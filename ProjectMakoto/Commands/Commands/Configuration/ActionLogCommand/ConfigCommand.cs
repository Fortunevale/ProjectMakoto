// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ActionLogCommand;

internal sealed class ConfigCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckAdmin();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = this.t.Commands.Config.ActionLog;

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            var embed = new DiscordEmbedBuilder
            {
                Description = ActionLogAbstractions.GetCurrentConfiguration(ctx)
            }.AsAwaitingInput(ctx, GetString(CommandKey.Title));

            var Disable = new DiscordButtonComponent(ButtonStyle.Danger, Guid.NewGuid().ToString(), GetString(CommandKey.DisableActionLogButton), (ctx.DbGuild.ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✖")));
            var ChangeChannel = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), $"{(ctx.DbGuild.ActionLog.Channel == 0 ? GetString(CommandKey.SetChannelButton) : GetString(CommandKey.ChangeChannelButton))}", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("💬")));
            var ChangeFilter = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), GetString(CommandKey.ChangeFilterButton), (ctx.DbGuild.ActionLog.Channel == 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📣")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed)
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
                ModifyToTimedOut(true);
                return;
            }

            _ = Button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (Button.GetCustomId() == Disable.CustomId)
            {
                ctx.DbGuild.ActionLog = new(ctx.Bot, ctx.DbGuild);

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeChannel.CustomId)
            {
                var ChannelResult = await PromptChannelSelection(ChannelType.Text, new ChannelPromptConfiguration
                {
                    CreateChannelOption = new()
                    {
                        Name = "actionlog",
                        ChannelType = ChannelType.Text
                    }
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
                        await RespondOrEdit(new DiscordEmbedBuilder().AsError(ctx).WithDescription(GetString(this.t.Commands.Common.Errors.NoChannels, true)));
                        await Task.Delay(3000);
                        await ExecuteCommand(ctx, arguments);
                        return;
                    }

                    throw ChannelResult.Exception;
                }

                await ChannelResult.Result.ModifyAsync(x => x.PermissionOverwrites = new List<DiscordOverwriteBuilder>
                {
                    new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole) { Denied = Permissions.All },
                    new DiscordOverwriteBuilder(ctx.Member) { Allowed = Permissions.All },
                });

                ctx.DbGuild.ActionLog.Channel = ChannelResult.Result.Id;

                await ExecuteCommand(ctx, arguments);
                return;
            }
            else if (Button.GetCustomId() == ChangeFilter.CustomId)
            {
                try
                {
                    var Selections = new List<DiscordStringSelectComponentOption>
                    {
                        new DiscordStringSelectComponentOption(GetString(CommandKey.AttemptGatheringMoreDetails), "attempt_further_detail", GetString(CommandKey.OptionInaccurate), ctx.DbGuild.ActionLog.AttemptGettingMoreDetails, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("⚠"))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.UserStateUpdates), "log_members_modified", null, ctx.DbGuild.ActionLog.MembersModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.UserRoleUpdates), "log_member_modified", null, ctx.DbGuild.ActionLog.MemberModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.UserProfileUpdates), "log_memberprofile_modified", null, ctx.DbGuild.ActionLog.MemberProfileModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.MessageDeletions), "log_message_deleted", null, ctx.DbGuild.ActionLog.MessageDeleted, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.MessageModifications), "log_message_updated", null, ctx.DbGuild.ActionLog.MessageModified, new DiscordComponentEmoji(EmojiTemplates.GetMessage(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.RoleUpdates), "log_roles_modified", null, ctx.DbGuild.ActionLog.RolesModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.BanUpdates), "log_banlist_modified", null, ctx.DbGuild.ActionLog.BanlistModified, new DiscordComponentEmoji(EmojiTemplates.GetUser(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.ServerModifications), "log_guild_modified", null, ctx.DbGuild.ActionLog.GuildModified, new DiscordComponentEmoji(EmojiTemplates.GetGuild(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.ChannelModifications), "log_channels_modified", null, ctx.DbGuild.ActionLog.ChannelsModified, new DiscordComponentEmoji(EmojiTemplates.GetChannel(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.VoiceChannelUpdates), "log_voice_state", null, ctx.DbGuild.ActionLog.VoiceStateUpdated, new DiscordComponentEmoji(EmojiTemplates.GetVoiceState(ctx.Bot))),
                        new DiscordStringSelectComponentOption(GetString(CommandKey.InviteModifications), "log_invites_modified", null, ctx.DbGuild.ActionLog.InvitesModified, new DiscordComponentEmoji(EmojiTemplates.GetInvite(ctx.Bot))),
                    };

                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new DiscordStringSelectComponent(GetString(CommandKey.NoOptions), Selections, Guid.NewGuid().ToString(), 0, Selections.Count, false)));

                    var e = await ctx.Client.GetInteractivity().WaitForSelectAsync(ctx.ResponseMessage, x => x.User.Id == ctx.User.Id, ComponentType.StringSelect, TimeSpan.FromMinutes(2));

                    if (e.TimedOut)
                    {
                        ModifyToTimedOut(true);
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    List<string> selected = e.Result.Values.ToList();

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

                    await ExecuteCommand(ctx, arguments);
                    return;
                }
                catch (ArgumentException)
                {
                    ModifyToTimedOut(true);
                    return;
                }
            }
            else if (Button.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser, ctx.Bot).CustomId)
            {
                DeleteOrInvalidate();
                return;
            }
        });
    }
}