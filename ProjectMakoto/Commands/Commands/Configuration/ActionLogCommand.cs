// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ActionLogCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string GetCurrentConfiguration(SharedCommandContext ctx)
            {
                var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.ActionLog;

                if (ctx.DbGuild.ActionLog.Channel == 0)
                    return $"❌ {CommandKey.ActionlogDisabled.Get(ctx.DbUser).Build(true)}";

                var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.InviteModifications, CommandKey.VoiceChannelUpdates, CommandKey.ChannelModifications, CommandKey.ServerModifications, CommandKey.BanUpdates,
                    CommandKey.RoleUpdates, CommandKey.MessageModifications, CommandKey.MessageModifications, CommandKey.UserProfileUpdates, CommandKey.UserRoleUpdates, CommandKey.UserStateUpdates,
                    CommandKey.AttemptGatheringMoreDetails, CommandKey.ActionLogChannel);

                return $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.ActionLogChannel.Get(ctx.DbUser).PadRight(pad)}` : <#{ctx.DbGuild.ActionLog.Channel}>\n\n" +
                       $"⚠ `{CommandKey.AttemptGatheringMoreDetails.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.AttemptGettingMoreDetails.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserStateUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.MembersModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserRoleUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.MemberModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.UserProfileUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.MemberProfileModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetMessage(ctx.Bot)} `{CommandKey.MessageDeletions.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.MessageDeleted.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetMessage(ctx.Bot)} `{CommandKey.MessageModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.MessageModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.RoleUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.RolesModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.BanUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.BanlistModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetGuild(ctx.Bot)} `{CommandKey.ServerModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.GuildModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.ChannelModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.ChannelsModified.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetVoiceState(ctx.Bot)} `{CommandKey.VoiceChannelUpdates.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.VoiceStateUpdated.ToEmote(ctx.Bot)}\n" +
                       $"{EmojiTemplates.GetInvite(ctx.Bot)} `{CommandKey.InviteModifications.Get(ctx.DbUser).PadRight(pad)}` : {ctx.DbGuild.ActionLog.InvitesModified.ToEmote(ctx.Bot)}";
            }

            var CommandKey = this.t.Commands.Config.ActionLog;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, this.GetString(CommandKey.Title));

            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), this.GetString(CommandKey.DisableActionLogButton), (ctx.DbGuild.ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"{(ctx.DbGuild.ActionLog.Channel == 0 ? this.GetString(CommandKey.SetChannelButton) : this.GetString(CommandKey.ChangeChannelButton))}", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeFilter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), this.GetString(CommandKey.ChangeFilterButton), (ctx.DbGuild.ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📣")));

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
            .AddComponents(new List<DiscordComponent>
            {
                { Disable }
            })
            .AddComponents(new List<DiscordComponent>
            {
                { ChangeChannel },
                { ChangeFilter }
            }).AddComponents(MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot)));

            var Button = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (Button.TimedOut)
            {
                this.ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.GetCustomId() == Disable.CustomId)
            {
                ctx.DbGuild.ActionLog.Channel = 0;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await this.PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = "actionlog",
                        ChannelType = ChannelType.Text
                    }
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
                        _ = await this.RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(this.GetString(this.t.Commands.Common.Errors.NoChannels, true)));
                        await Task.Delay(3000);
                        await this.ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                await ChannelResult.Result.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                {
                    new(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                    new(ctx.Member) { Allowed = Permissions.All },
                });

                ctx.DbGuild.ActionLog.Channel = ChannelResult.Result.Id;

                await this.ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeFilter.CustomId)
            {
                try
                {
                    var Selections = new List<DiscordStringSelectComponentOption>
                    {
                        new(this.GetString(CommandKey.AttemptGatheringMoreDetails), "attempt_further_detail", this.GetString(CommandKey.OptionInaccurate), ctx.DbGuild.ActionLog.AttemptGettingMoreDetails, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠"))),
                        new(this.GetString(CommandKey.UserStateUpdates), "log_members_modified", null, ctx.DbGuild.ActionLog.MembersModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new(this.GetString(CommandKey.UserRoleUpdates), "log_member_modified", null, ctx.DbGuild.ActionLog.MemberModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new(this.GetString(CommandKey.UserProfileUpdates), "log_memberprofile_modified", null, ctx.DbGuild.ActionLog.MemberProfileModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new(this.GetString(CommandKey.MessageDeletions), "log_message_deleted", null, ctx.DbGuild.ActionLog.MessageDeleted, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new(this.GetString(CommandKey.MessageModifications), "log_message_updated", null, ctx.DbGuild.ActionLog.MessageModified, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new(this.GetString(CommandKey.RoleUpdates), "log_roles_modified", null, ctx.DbGuild.ActionLog.RolesModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new(this.GetString(CommandKey.BanUpdates), "log_banlist_modified", null, ctx.DbGuild.ActionLog.BanlistModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new(this.GetString(CommandKey.ServerModifications), "log_guild_modified", null, ctx.DbGuild.ActionLog.GuildModified, new DiscordComponentEmoji(EmojiTemplates.GetGuild(ctx.Bot))),
                        new(this.GetString(CommandKey.ChannelModifications), "log_channels_modified", null, ctx.DbGuild.ActionLog.ChannelsModified, new DiscordComponentEmoji(EmojiTemplates.GetChannel(ctx.Bot))),
                        new(this.GetString(CommandKey.VoiceChannelUpdates), "log_voice_state", null, ctx.DbGuild.ActionLog.VoiceStateUpdated, new DiscordComponentEmoji(EmojiTemplates.GetVoiceState(ctx.Bot))),
                        new(this.GetString(CommandKey.InviteModifications), "log_invites_modified", null, ctx.DbGuild.ActionLog.InvitesModified, new DiscordComponentEmoji(EmojiTemplates.GetInvite(ctx.Bot))),
                    };

                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new DiscordStringSelectComponent(this.GetString(CommandKey.NoOptions), Selections, Guid.NewGuid().ToString(), 0, Selections.Count, false)));

                    var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect, TimeSpan.FromMinutes(2));

                    if (e.TimedOut)
                    {
                        this.ModifyToTimedOut(true);
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    var selected = e.Result.Values.ToList();

                    ctx.DbGuild.ActionLog.AttemptGettingMoreDetails = selected.Contains("attempt_further_detail");

                    ctx.DbGuild.ActionLog.MembersModified = selected.Contains("log_members_modified");
                    ctx.DbGuild.ActionLog.MemberModified = selected.Contains("log_member_modified");
                    ctx.DbGuild.ActionLog.MemberProfileModified = selected.Contains("log_memberprofile_modified");
                    ctx.DbGuild.ActionLog.MessageDeleted = selected.Contains("log_message_deleted");
                    ctx.DbGuild.ActionLog.MessageModified = selected.Contains("log_message_updated");
                    ctx.DbGuild.ActionLog.RolesModified = selected.Contains("log_roles_modified");
                    ctx.DbGuild.ActionLog.BanlistModified = selected.Contains("log_banlist_modified");
                    ctx.DbGuild.ActionLog.GuildModified = selected.Contains("log_guild_modified");
                    ctx.DbGuild.ActionLog.ChannelsModified = selected.Contains("log_channels_modified");
                    ctx.DbGuild.ActionLog.VoiceStateUpdated = selected.Contains("log_voice_state");
                    ctx.DbGuild.ActionLog.InvitesModified = selected.Contains("log_invites_modified");

                    await this.ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    this.ModifyToTimedOut(true);
                    return;
                }
            }
            else if (Button.GetCustomId() == MessageComponents.CancelButtonId)
            {
                this.DeleteOrInvalidate();
                return;
            }
        });
    }
}