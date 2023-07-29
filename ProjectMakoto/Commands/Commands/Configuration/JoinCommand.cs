// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class JoinCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.Join;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.Join;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Autoban, CommandKey.JoinLogChannel, CommandKey.Role, CommandKey.ReApplyRoles, CommandKey.ReApplyNickname);

                return $"{"🌐".UnicodeToEmoji()} `{CommandKey.Autoban.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.AutoBanGlobalBans.ToEmote(ctx.Bot)}\n" +
                        $"{"👋".UnicodeToEmoji()} `{CommandKey.JoinLogChannel.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.JoinlogChannelId != 0 ? $"<#{ctx.DbGuild.Join.JoinlogChannelId}>" : false.ToEmote(ctx.Bot))}\n" +
                        $"{"👤".UnicodeToEmoji()} `{CommandKey.Role.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.AutoAssignRoleId != 0 ? $"<@&{ctx.DbGuild.Join.AutoAssignRoleId}>" : false.ToEmote(ctx.Bot))}\n" +
                        $"{"👥".UnicodeToEmoji()} `{CommandKey.ReApplyRoles.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyRoles.ToEmote(ctx.Bot)}\n" +
                        $"{"💬".UnicodeToEmoji()} `{CommandKey.ReApplyNickname.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyNickname.ToEmote(ctx.Bot)}\n\n" +
                        $"{CommandKey.SecurityNotice.Get(ctx.DbUser).Build(true, new TVar("Permissions", string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToTranslatedPermissionString(ctx.DbUser, ctx.Bot)}`"))))}\n\n" +
                        $"{CommandKey.TimeNotice.Get(ctx.DbUser).Build()}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleGlobalban = new DiscordButtonComponent((ctx.DbGuild.Join.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleGlobalBansButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🌐")));
            var ChangeJoinlogChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeJoinlogChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👋")));
            var ChangeRoleOnJoin = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeRoleButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            var ToggleReApplyRoles = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleReApplyRole), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
            var ToggleReApplyName = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), GetString(CommandKey.ToggleReApplyNickname), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            await RespondOrEdit(builder
            .AddComponents(new List<DiscordComponent>
            {
                ToggleGlobalban,
                ToggleReApplyRoles,
                ToggleReApplyName,
            })
            .AddComponents(new List<DiscordComponent>
            {
                ChangeJoinlogChannel,
                ChangeRoleOnJoin,
            })
            .AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleGlobalban.CustomId)
            {
                ctx.DbGuild.Join.AutoBanGlobalBans = !ctx.DbGuild.Join.AutoBanGlobalBans;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleReApplyRoles.CustomId)
            {
                ctx.DbGuild.Join.ReApplyRoles = !ctx.DbGuild.Join.ReApplyRoles;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleReApplyName.CustomId)
            {
                ctx.DbGuild.Join.ReApplyNickname = !ctx.DbGuild.Join.ReApplyNickname;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeJoinlogChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = GetString(CommandKey.JoinLogChannelName),
                        ChannelType = ChannelType.Text
                    },
                    DisableOption = GetString(CommandKey.DisableJoinlog)
                });

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

                ctx.DbGuild.Join.JoinlogChannelId = ChannelResult.Result is null ? 0 : ChannelResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeRoleOnJoin.CustomId)
            {
                var RoleResult = await PromptRoleSelection(new RolePromptConfiguration { CreateRoleOption = GetString(CommandKey.AutoAssignRoleName), DisableOption = GetString(CommandKey.DisableRoleOnJoin) });

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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoRoles)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                if (RoleResult.Result?.Id == ctx.DbGuild.BumpReminder.RoleId)
                {
                    await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(CommandKey.CantUseRole, true)));
                    await Task.Delay(3000);
                    await ExecuteCommand(ctx, arguments);
                    return;
                }

                ctx.DbGuild.Join.AutoAssignRoleId = RoleResult.Result is null ? 0 : RoleResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}