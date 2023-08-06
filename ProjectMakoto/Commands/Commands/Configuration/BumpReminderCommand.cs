// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class BumpReminderCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.BumpReminder;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.BumpReminder;

                if (!ctx.DbGuild.BumpReminder.Enabled)
                    return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `{CommandKey.BumpReminderEnabled.Get(ctx.DbUser)}` : {ctx.DbGuild.BumpReminder.Enabled.ToEmote(ctx.Bot)}";

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.BumpReminderEnabled, CommandKey.BumpReminderChannel, CommandKey.BumpReminderRole);

                return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `{CommandKey.BumpReminderEnabled.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.BumpReminder.Enabled.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.BumpReminderChannel.Get(ctx.DbUser).PadRight(pad)}` : <#{ctx.DbGuild.BumpReminder.ChannelId}> `({ctx.DbGuild.BumpReminder.ChannelId})`\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.BumpReminderRole.Get(ctx.DbUser).PadRight(pad)}` : <@&{ctx.DbGuild.BumpReminder.RoleId}> `({ctx.DbGuild.BumpReminder.RoleId})`";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var Setup = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), this.GetString(CommandKey.SetupBumpReminderButton), ctx.DbGuild.BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("➕")));
            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(CommandKey.DisableBumpReminderButton), !ctx.DbGuild.BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeChannelButton), !ctx.DbGuild.BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeRole = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeRoleButton), !ctx.DbGuild.BumpReminder.Enabled, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                .WithDescription(GetCurrentConfiguration(ctx)).AsAwaitingInput(ctx, this.GetString(CommandKey.Title)))
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
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == Setup.CustomId)
            {
                if (!(await ctx.Guild.GetAllMembersAsync()).Any(x => x.Id == ctx.Bot.status.LoadedConfig.Accounts.Disboard))
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                        .WithDescription(this.GetString(CommandKey.DisboardMissing, true))
                        .AsError(ctx, this.GetString(CommandKey.Title)));
                    return;
                }

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.SettingUp, true))
                    .AsLoading(ctx, this.GetString(CommandKey.Title)));

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.SelectRole, true))
                    .AsAwaitingInput(ctx, this.GetString(CommandKey.Title)));


                var RoleResult = await this.PromptRoleSelection(new() { CreateRoleOption = "BumpReminder" });

                if (RoleResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoChannels, true)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                if (RoleResult.Result.Id == ctx.DbGuild.Join.AutoAssignRoleId || ctx.DbGuild.LevelRewards.Any(x => x.RoleId == RoleResult.Result.Id))
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.CantUseRole, true)));
                    await Task.Delay(3000);
                    return;
                }

                var bump_reaction_msg = await ctx.Channel.SendMessageAsync(this.GetGuildString(CommandKey.ReactionRoleMessage, new TVar("Emoji", "✅".UnicodeToEmoji())));
                _ = bump_reaction_msg.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
                _ = bump_reaction_msg.PinAsync();

                _ = ctx.Channel.DeleteMessagesAsync((await ctx.Channel.GetMessagesAsync(2)).Where(x => x.Author.Id == ctx.Client.CurrentUser.Id && x.MessageType == MessageType.ChannelPinnedMessage));

                ctx.DbGuild.BumpReminder.RoleId = RoleResult.Result.Id;
                ctx.DbGuild.BumpReminder.ChannelId = ctx.Channel.Id;
                ctx.DbGuild.BumpReminder.MessageId = bump_reaction_msg.Id;
                ctx.DbGuild.BumpReminder.LastBump = DateTime.UtcNow.AddHours(-2);
                ctx.DbGuild.BumpReminder.LastReminder = DateTime.UtcNow.AddHours(-2);
                ctx.DbGuild.BumpReminder.LastUserId = 0;

                ctx.DbGuild.BumpReminder.Enabled = true;

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.SetupComplete, true))
                    .AsSuccess(ctx, this.GetString(CommandKey.Title)));

                await Task.Delay(5000);
                ctx.Bot.BumpReminder.SendPersistentMessage(ctx.Client, ctx.Channel, null);
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == Disable.CustomId)
            {
                ctx.DbGuild.BumpReminder = new(ctx.Bot, ctx.DbGuild);

                foreach (var b in ScheduledTaskExtensions.GetScheduledTasks())
                {
                    if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                        continue;

                    if (scheduledTaskIdentifier.Snowflake == ctx.Guild.Id && scheduledTaskIdentifier.Type == "bumpmsg")
                        b.Delete();
                }

                _ = await this.RespondOrEdit(new DiscordEmbedBuilder()
                    .WithDescription(this.GetString(CommandKey.DisableBumpReminderButton, true))
                    .AsSuccess(ctx, this.GetString(CommandKey.Title)));

                await Task.Delay(5000);
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await this.PromptChannelSelection(ChannelType.Text);

                if (ChannelResult.TimedOut)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
                else if (ChannelResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (ChannelResult.Failed)
                {
                    if (ChannelResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoChannels)));
                        await Task.Delay(3000);
                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                ctx.DbGuild.BumpReminder.ChannelId = ChannelResult.Result.Id;
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeRole.CustomId)
            {

                var RoleResult = await this.PromptRoleSelection();

                if (RoleResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (RoleResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (RoleResult.Failed)
                {
                    if (RoleResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoRoles, true)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                ctx.DbGuild.BumpReminder.RoleId = RoleResult.Result.Id;
                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}