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
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await this.CheckAdmin();

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
                        $"{CommandKey.SecurityNotice.Get(ctx.DbUser).Build(true, new TVar("Permissions", string.Join(", ", Resources.ProtectedPermissions.Select(x => $"{x.ToTranslatedPermissionString(ctx.DbUser, ctx.Bot)}"))))}\n\n" +
                        $"{CommandKey.TimeNotice.Get(ctx.DbUser).Build()}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var builder = new DiscordMessageBuilder().WithEmbed(embed);

            var ToggleGlobalban = new DiscordButtonComponent((ctx.DbGuild.Join.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleGlobalBansButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🌐")));
            var ChangeJoinlogChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeJoinlogChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👋")));
            var ChangeRoleOnJoin = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeRoleButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            var ToggleReApplyRoles = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleReApplyRole), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
            var ToggleReApplyName = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleReApplyNickname), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));

            _ = await this.RespondOrEdit(builder
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
                this.ModifyToTimedOut(true);
                return;
            }

            _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ToggleGlobalban.CustomId)
            {
                ctx.DbGuild.Join.AutoBanGlobalBans = !ctx.DbGuild.Join.AutoBanGlobalBans;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleReApplyRoles.CustomId)
            {
                ctx.DbGuild.Join.ReApplyRoles = !ctx.DbGuild.Join.ReApplyRoles;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ToggleReApplyName.CustomId)
            {
                ctx.DbGuild.Join.ReApplyNickname = !ctx.DbGuild.Join.ReApplyNickname;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeJoinlogChannel.CustomId)
            {
                var ChannelResult = await this.PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = this.GetString(CommandKey.JoinLogChannelName),
                        ChannelType = ChannelType.Text
                    },
                    DisableOption = this.GetString(CommandKey.DisableJoinlog)
                });

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

                ctx.DbGuild.Join.JoinlogChannelId = ChannelResult.Result is null ? 0 : ChannelResult.Result.Id;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeRoleOnJoin.CustomId)
            {
                var RoleResult = await this.PromptRoleSelection(new RolePromptConfiguration { CreateRoleOption = this.GetString(CommandKey.AutoAssignRoleName), DisableOption = this.GetString(CommandKey.DisableRoleOnJoin) });

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
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoRoles)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw RoleResult.Exception;
                }

                if (RoleResult.Result?.Id == ctx.DbGuild.BumpReminder.RoleId)
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.CantUseRole, true)));
                    await Task.Delay(3000);
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }

                ctx.DbGuild.Join.AutoAssignRoleId = RoleResult.Result is null ? 0 : RoleResult.Result.Id;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                this.DeleteOrInvalidate();
            }
        });
    }
}