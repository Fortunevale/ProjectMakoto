// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.BumpReminderCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.BumpReminder;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var Setup = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(CommandKey.SetupBumpReminderButton), ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(CommandKey.DisableBumpReminderButton), !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeChannelButton), !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeRole = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeRoleButton), !ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                .WithDescription(BumpReminderCommandAbstractions.GetCurrentConfiguration(ctx)).AsAwaitingInput(ctx, GetString(CommandKey.Title)))
            .AddComponents(new List<DiscordComponent>
            {
                { Setup },
                { Disable }
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangeChannel },
                { ChangeRole }
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Setup.CustomId)
            {
                if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == ctx.Bot.status.LoadedConfig.Accounts.Disboard))
                {
                    await RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(GetString(CommandKey.DisboardMissing, true))
                        .AsError(ctx, GetString(CommandKey.Title)));
                    return;
                }

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.SettingUp, true))
                    .AsLoading(ctx, GetString(CommandKey.Title)));

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.SelectRole, true))
                    .AsAwaitingInput(ctx, GetString(CommandKey.Title)));


                var RoleResult = await PromptRoleSelection(new() { CreateRoleOption = "BumpReminder" });

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoChannels, true)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                if (RoleResult.Result.Id == ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId || ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Any(x => x.RoleId == RoleResult.Result.Id))
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.CantUseRole, true)));
                    await Task.Delay(3000);
                    return;
                }

                var bump_reaction_msg = await ctx.Channel.SendMessageAsync(GetGuildString(CommandKey.ReactionRoleMessage, new TVar("Emoji", "✅".UnicodeToEmoji())));
                _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
                _ = bump_reaction_msg.PinAsync();

                _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId = RoleResult.Result.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId = ctx.Channel.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.MessageId = bump_reaction_msg.Id;
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastBump = DateTime.UtcNow.AddHours(-2);
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastReminder = DateTime.UtcNow.AddHours(-2);
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.LastUserId = 0;

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled = true;

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.SetupComplete, true))
                    .AsSuccess(ctx, GetString(CommandKey.Title)));

                await Task.Delay(5000);
                ctx.Bot.bumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == Disable.CustomId)
            {
                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder = new(ctx.Bot.guilds[ctx.Guild.Id]);

                foreach (var b in UniversalExtensions.GetScheduledTasks())
                {
                    if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                        continue;

                    if (scheduledTaskIdentifier.Snowflake == ctx.Guild.Id && scheduledTaskIdentifier.Type == "bumpmsg")
                        b.Delete();
                }

                await RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(GetString(CommandKey.DisableBumpReminderButton, true))
                    .AsSuccess(ctx, GetString(CommandKey.Title)));

                await Task.Delay(5000);
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text);

                if (ChannelResult.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoChannels)));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId = ChannelResult.Result.Id;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeRole.CustomId)
            {

                var RoleResult = await PromptRoleSelection();

                if (RoleResult.TimedOut)
                {
                    ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoRoles, true)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId = RoleResult.Result.Id;
                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}