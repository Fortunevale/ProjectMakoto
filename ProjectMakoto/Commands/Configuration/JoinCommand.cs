// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.Configuration;

internal sealed class JoinCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.Join;

            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.Join;

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, 
                    CommandKey.Autoban, 
                    CommandKey.JoinLogChannel, 
                    CommandKey.Role, 
                    CommandKey.ReApplyRoles, 
                    CommandKey.ReApplyNickname, 
                    CommandKey.AutoKickSpammer,
                    CommandKey.AutoKickNewAccounts,
                    CommandKey.AutoKickNoRoles);

                return $"{"🌐".UnicodeToEmoji()} `{CommandKey.Autoban.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.AutoBanGlobalBans.ToEmote(ctx.Bot)}\n" +
                        $"{"👋".UnicodeToEmoji()} `{CommandKey.JoinLogChannel.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.JoinlogChannelId != 0 ? $"<#{ctx.DbGuild.Join.JoinlogChannelId}>" : false.ToEmote(ctx.Bot))}\n" +
                        $"{"👤".UnicodeToEmoji()} `{CommandKey.Role.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.AutoAssignRoleId != 0 ? $"<@&{ctx.DbGuild.Join.AutoAssignRoleId}>" : false.ToEmote(ctx.Bot))}\n" +
                        $"{"👥".UnicodeToEmoji()} `{CommandKey.ReApplyRoles.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyRoles.ToEmote(ctx.Bot)}\n" +
                        $"{"💬".UnicodeToEmoji()} `{CommandKey.ReApplyNickname.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyNickname.ToEmote(ctx.Bot)}\n\n" +
                        $"{"⚠️".UnicodeToEmoji()} `{CommandKey.AutoKickSpammer.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.AutoKickSpammer.ToEmote(ctx.Bot)}\n" +
                        $"{"🕒".UnicodeToEmoji()} `{CommandKey.AutoKickNewAccounts.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.AutoKickAccountAge == TimeSpan.Zero ? false.ToEmote(ctx.Bot) : $"`{ctx.DbGuild.Join.AutoKickAccountAge.GetHumanReadable(TimeFormat.Days, TranslationUtil.GetTranslatedHumanReadableConfig(ctx.DbUser, ctx.Bot))}`")}\n" +
                        $"{"📝".UnicodeToEmoji()} `{CommandKey.AutoKickNoRoles.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.AutoKickNoRoleTime == TimeSpan.Zero ? false.ToEmote(ctx.Bot) : $"`{ctx.DbGuild.Join.AutoKickNoRoleTime.GetHumanReadable(TimeFormat.Minutes, TranslationUtil.GetTranslatedHumanReadableConfig(ctx.DbUser, ctx.Bot))}`")}\n\n" +
                        $"{CommandKey.SecurityNotice.Get(ctx.DbUser).Build(true, new TVar("Permissions", string.Join(", ", Resources.ProtectedPermissions.Select(x => $"{x.ToTranslatedPermissionString(ctx.DbUser, ctx.Bot)}"))))}\n\n" +
                        $"{CommandKey.TimeNotice.Get(ctx.DbUser).Build()}";
            }

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder()
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var builder = new DiscordMessageBuilder().AddEmbed(embed);

            var ToggleGlobalban = new DiscordButtonComponent((ctx.DbGuild.Join.AutoBanGlobalBans ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleGlobalBansButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🌐")));
            var ChangeJoinlogChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeJoinlogChannelButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👋")));
            var ChangeRoleOnJoin = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeRoleButton), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            var ToggleReApplyRoles = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyRoles ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleReApplyRole), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👥")));
            var ToggleReApplyName = new DiscordButtonComponent((ctx.DbGuild.Join.ReApplyNickname ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleReApplyNickname), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            
            var ToggleAutoKickSpammer = new DiscordButtonComponent((ctx.DbGuild.Join.AutoKickSpammer ? ButtonStyle.Danger : ButtonStyle.Success), Guid.NewGuid().ToString(), this.GetString(CommandKey.ToggleAutoKickSpammer), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠️")));
            var ChangeAutoKickNewAccounts = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeAutoKickNewAccounts), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));
            var ChangeAutoKickNoRoles = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeAutoKickNoRoles), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📝")));

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
            .AddComponents(new List<DiscordComponent>
            {
                ToggleAutoKickSpammer,
                ChangeAutoKickNewAccounts,
                ChangeAutoKickNoRoles,
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
            else if (e.GetCustomId() == ToggleAutoKickSpammer.CustomId)
            {
                ctx.DbGuild.Join.AutoKickSpammer = !ctx.DbGuild.Join.AutoKickSpammer;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeAutoKickNewAccounts.CustomId)
            {
                var TimeResult = await this.PromptForTimeSpan(TimeSpan.FromDays(30), TimeSpan.Zero, ctx.DbGuild.Join.AutoKickAccountAge);

                if (TimeResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (TimeResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (TimeResult.Failed)
                {
                    if (TimeResult.Exception.GetType() == typeof(InvalidOperationException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.AutoKickNewAccountsDurationLimit)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw TimeResult.Exception;
                }

                ctx.DbGuild.Join.AutoKickAccountAge = TimeResult.Result;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == ChangeAutoKickNoRoles.CustomId)
            {
                var TimeResult = await this.PromptForTimeSpan(TimeSpan.FromMinutes(30), TimeSpan.Zero, ctx.DbGuild.Join.AutoKickNoRoleTime);

                if (TimeResult.TimedOut)
                {
                    this.ModifyToTimedOut();
                    return;
                }
                else if (TimeResult.Cancelled)
                {
                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                else if (TimeResult.Failed)
                {
                    if (TimeResult.Exception.GetType() == typeof(NullReferenceException))
                    {
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(CommandKey.AutoKickNoRolesDurationLimit)));
                        await Task.Delay(3000);
                        return;
                    }

                    throw TimeResult.Exception;
                }

                if (TimeResult.Result < TimeSpan.FromMinutes(1) && TimeResult.Result != TimeSpan.Zero)
                {
                    _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsWarning(ctx).WithDescription($"{this.GetString(CommandKey.LowTimeWarning, true, new TVar("Time", TimeResult.Result.GetHumanReadable(TimeFormat.Minutes, TranslationUtil.GetTranslatedHumanReadableConfig(ctx.DbUser, ctx.Bot))))}\n\n" +
                        $"{this.GetString(this.t.Commands.Moderation.CustomEmbed.ContinueTimer, true, new TVar("Timestamp", DateTime.UtcNow.AddSeconds(6).ToTimestamp()))}"));
                    await Task.Delay(5000);
                }

                ctx.DbGuild.Join.AutoKickNoRoleTime = TimeResult.Result;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (e.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
            }
        });
    }
}