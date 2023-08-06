// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class UserInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var victim = (DiscordUser)arguments["victim"];

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            victim ??= ctx.User;

            victim = await victim.GetFromApiAsync();

            DiscordMember? bMember = null;

            try
            {
                bMember = await ctx.Guild.GetMemberAsync(victim.Id);
            }
            catch { }

            static string GetStatusIcon(UserStatus? status)
            {
                return status switch
                {
                    UserStatus.Online => "🟢",
                    UserStatus.DoNotDisturb => "🔴",
                    UserStatus.Idle => "🟡",
                    UserStatus.Streaming => "🟣",
                    _ => "⚪",
                };
            }

            var GenerateRoles = "";

            if (bMember is not null)
            {
                GenerateRoles = bMember.Roles.Any() ? string.Join(", ", bMember.Roles.Select(x => x.Mention)) : "`User doesn't have any roles.`";
            }
            else
            {
                if (!ctx.DbGuild.Members.ContainsKey(victim.Id))
                    ctx.DbGuild.Members.Add(victim.Id, new(ctx.Bot, ctx.DbGuild, victim.Id));

                GenerateRoles = ctx.DbGuild.Members[victim.Id].MemberRoles.Count > 0
                    ? string.Join(", ", ctx.DbGuild.Members[victim.Id].MemberRoles.Where(x => ctx.Guild.Roles.ContainsKey(x.Id)).Select(x => $"{ctx.Guild.GetRole(x.Id).Mention}"))
                    : "`User doesn't have any stored roles.`";
            }

            var banList = await ctx.Guild.GetBansAsync();
            var isBanned = banList.Any(x => x.User.Id == victim.Id);
            var banDetails = (isBanned ? banList.First(x => x.User.Id == victim.Id) : null);

            var builder = new DiscordMessageBuilder();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{(victim.IsBot ? $"[{(victim.IsSystem ?? false ? this.GetString(this.t.Commands.Utility.UserInfo.System) : $"{this.GetString(this.t.Commands.Utility.UserInfo.Bot)}{(victim.IsVerifiedBot ? "✅" : "❎")}")}] " : "")}{victim.GetUsernameWithIdentifier()}",
                    Url = victim.ProfileUrl
                },
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = (string.IsNullOrWhiteSpace(victim.AvatarUrl) ? "https://cdn.discordapp.com/attachments/712761268393738301/899051918037504040/QuestionMark.png" : victim.AvatarUrl)
                },
                Color = victim.BannerColor ?? new("2f3136"),
                ImageUrl = victim.BannerUrl,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"User-Id: {victim.Id}"
                },
                Description = $"{(bMember is null ? $"{(ctx.DbGuild.Members[victim.Id].FirstJoinDate == DateTime.UnixEpoch ? this.GetString(this.t.Commands.Utility.UserInfo.NeverJoined, true) : $"{(isBanned ? this.GetString(this.t.Commands.Utility.UserInfo.IsBanned, true) : this.GetString(this.t.Commands.Utility.UserInfo.JoinedBefore, true))}")}\n\n" : "")}" +
                        $"{(ctx.Bot.globalBans.ContainsKey(victim.Id) ? $"💀 **{this.GetString(this.t.Commands.Utility.UserInfo.GlobalBanned, true)}**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamOwner == victim.Id ? $"👑 **{this.GetString(this.t.Commands.Utility.UserInfo.BotOwner, true)}**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamMembers.Contains(victim.Id) ? $"🔏 **{this.GetString(this.t.Commands.Utility.UserInfo.BotStaff, true)}**\n\n" : "")}" +
                        $"{(bMember is not null && bMember.IsOwner ? $"✨ {this.GetString(this.t.Commands.Utility.UserInfo.Owner, true)}\n" : "")}" +
                        $"{(victim.IsStaff ? $"📘 **{this.GetString(this.t.Commands.Utility.UserInfo.DiscordStaff, true)}**\n" : "")}" +
                        $"{(victim.IsMod ? $"⚒ {this.GetString(this.t.Commands.Utility.UserInfo.CertifiedMod, true)}\n" : "")}" +
                        $"{(victim.IsBotDev ? $"⌨ {this.GetString(this.t.Commands.Utility.UserInfo.VerifiedBotDeveloper, true)}\n" : "")}" +
                        $"{(victim.IsPartner ? $"👥 {this.GetString(this.t.Commands.Utility.UserInfo.DiscordPartner, true)}\n" : "")}" +
                        $"{(bMember is not null && bMember.IsPending.HasValue && bMember.IsPending.Value ? $"❗ {this.GetString(this.t.Commands.Utility.UserInfo.PendingMembership, true)}\n" : "")}" +
                        $"\n**{(bMember is null ? $"{this.GetString(this.t.Commands.Utility.UserInfo.Roles)} ({this.GetString(this.t.Commands.Utility.UserInfo.Backup)})" : this.GetString(this.t.Commands.Utility.UserInfo.Roles))}**\n{GenerateRoles}"
            };

            if (ctx.Bot.globalNotes.TryGetValue(victim.Id, out var globalNotes) && globalNotes.Any())
            {
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.BotNotes), $"{string.Join("\n\n", ctx.Bot.globalNotes[victim.Id].Select(x => $"{x.Reason.FullSanitize()} - <@{x.Moderator}> {x.Timestamp.ToTimestamp()}"))}".TruncateWithIndication(512)));
            }

            if (ctx.Bot.globalBans.TryGetValue(victim.Id, out var globalBanDetails))
            {
                var gBanMod = await ctx.Client.GetUserAsync(ctx.Bot.globalBans[victim.Id].Moderator);

                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.GlobalBanReason), $"`{((string.IsNullOrWhiteSpace(globalBanDetails.Reason) || globalBanDetails.Reason == "-") ? this.GetString(this.t.Commands.Utility.UserInfo.NoReason) : globalBanDetails.Reason).SanitizeForCode()}`", true));
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.GlobalBanMod), $"`{gBanMod.GetUsernameWithIdentifier()}`", true));
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.GlobalBanDate), $"{Formatter.Timestamp(globalBanDetails.Timestamp)} ({Formatter.Timestamp(globalBanDetails.Timestamp, TimestampFormat.LongDateTime)})", true));
            }

            if (isBanned)
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.BanDetails), $"`{(string.IsNullOrWhiteSpace(banDetails?.Reason) ? this.GetString(this.t.Commands.Utility.UserInfo.NoReason) : $"{banDetails.Reason}")}`", false));

            var InviterButtonAdded = false;

            if (ctx.DbGuild.InviteTracker.Enabled)
            {
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.InvitedBy), $"{(ctx.DbGuild.Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace() ? this.GetString(this.t.Commands.Utility.UserInfo.NoInviter, true) : $"<@{ctx.DbGuild.Members[victim.Id].InviteTracker.UserId}> (`{ctx.DbGuild.Members[victim.Id].InviteTracker.UserId}`)")}", true));
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.UsersInvited), $"`{(ctx.DbGuild.Members.Where(b => b.Value.InviteTracker.UserId == victim.Id)).Count()}`", true));

                if (!ctx.DbGuild.Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace())
                {
                    InviterButtonAdded = true;
                    _ = builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, $"userinfo-inviter", this.GetString(this.t.Commands.Utility.UserInfo.ShowProfileInviter), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤"))));
                }
            }

            if (bMember is not null)
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.ServerJoinDate), $"{Formatter.Timestamp(bMember.JoinedAt, TimestampFormat.LongDateTime)}", true));
            else
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.ServerLeaveDate), (ctx.DbGuild.Members[victim.Id].LastLeaveDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.DbGuild.Members[victim.Id].LastLeaveDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.DbGuild.Members[victim.Id].LastLeaveDate)})" : "`User never joined this server.`"), true));

            _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.FirstJoinDate), (ctx.DbGuild.Members[victim.Id].FirstJoinDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.DbGuild.Members[victim.Id].FirstJoinDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.DbGuild.Members[victim.Id].FirstJoinDate)})" : "`User never joined this server.`"), true));

            _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.AccountCreationDate), $"{Formatter.Timestamp(victim.CreationTimestamp, TimestampFormat.LongDateTime)}", true));

            if (bMember is not null && bMember.PremiumSince.HasValue)
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.ServerBoosterSince), $"{Formatter.Timestamp(bMember.PremiumSince.Value, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Pronouns))
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.Pronouns), $"`{victim.Pronouns}`", true));

            if (victim.BannerColor is not null)
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.BannerColor), $"`{victim.BannerColor.Value}`", true));

            string TranslatePresence(UserStatus status)
            {
                return status switch
                {
                    UserStatus.Online => this.GetString(this.t.Commands.Utility.UserInfo.Online),
                    UserStatus.Idle => this.GetString(this.t.Commands.Utility.UserInfo.Idle),
                    UserStatus.DoNotDisturb => this.GetString(this.t.Commands.Utility.UserInfo.DoNotDisturb),
                    UserStatus.Streaming => this.GetString(this.t.Commands.Utility.UserInfo.Streaming),
                    UserStatus.Offline => this.GetString(this.t.Commands.Utility.UserInfo.Offline),
                    UserStatus.Invisible => this.GetString(this.t.Commands.Utility.UserInfo.Offline),
                    _ => status.ToString(),
                };
            }

            try
            {
                if (victim.Presence is not null)
                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.Presence), $"{GetStatusIcon(victim.Presence.Status)} `{TranslatePresence(victim.Presence.Status)}`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Desktop.HasValue ? victim.Presence.ClientStatus.Desktop.Value : UserStatus.Offline)} {this.GetString(this.t.Commands.Utility.UserInfo.Desktop, true)}\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Mobile.HasValue ? victim.Presence.ClientStatus.Mobile.Value : UserStatus.Offline)} {this.GetString(this.t.Commands.Utility.UserInfo.Mobile, true)}\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Web.HasValue ? victim.Presence.ClientStatus.Web.Value : UserStatus.Offline)} {this.GetString(this.t.Commands.Utility.UserInfo.Web, true)}\n\n", true));
            }
            catch { }

            string TranslateActivity(ActivityType type)
            {
                return type switch
                {
                    ActivityType.Playing => this.GetString(this.t.Commands.Utility.UserInfo.Playing),
                    ActivityType.Streaming => this.GetString(this.t.Commands.Utility.UserInfo.Streaming),
                    ActivityType.ListeningTo => this.GetString(this.t.Commands.Utility.UserInfo.ListeningTo),
                    ActivityType.Watching => this.GetString(this.t.Commands.Utility.UserInfo.Watching),
                    ActivityType.Competing => this.GetString(this.t.Commands.Utility.UserInfo.Competing),
                    _ => type.ToString(),
                };
            }

            try
            {
                if (victim.Presence is not null && victim.Presence.Activities is not null && victim.Presence.Activities?.Count > 0)
                    _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.Activities), string.Join("\n", victim.Presence.Activities.Select(x => $"{(x.ActivityType == ActivityType.Custom ? $"• {this.GetString(this.t.Commands.Utility.UserInfo.Status)}: `{x.CustomStatus.Emoji?.Name ?? "None"}`{(string.IsNullOrWhiteSpace(x.CustomStatus.Name) ? "" : $" {x.CustomStatus.Name}")}\n" : $"• {TranslateActivity(x.ActivityType)} {x.Name}")}")), true));
            }
            catch { }

            if (bMember is not null && bMember.CommunicationDisabledUntil.HasValue && bMember.CommunicationDisabledUntil.Value.GetTotalSecondsUntil() > 0)
                _ = embed.AddField(new DiscordEmbedField(this.GetString(this.t.Commands.Utility.UserInfo.TimedOutUntil), $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value, TimestampFormat.LongDateTime)}", true));

            _ = await this.RespondOrEdit(builder.WithEmbed(embed));

            if (InviterButtonAdded)
            {
                _ = ctx.ResponseMessage.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(15)).ContinueWith(async x =>
                {
                    if (x.IsFaulted)
                        return;

                    var e = x.Result;

                    if (e.TimedOut)
                    {
                        this.ModifyToTimedOut();
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    DiscordUser newVictim;

                    try
                    {
                        newVictim = await ctx.Client.GetUserAsync(ctx.DbGuild.Members[victim.Id].InviteTracker.UserId);
                    }
                    catch (Exception)
                    {
                        _ = e.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                            .AddEmbed(new DiscordEmbedBuilder().WithDescription(this.GetString(this.t.Commands.Utility.UserInfo.FetchUserError, true, new TVar("User", ctx.DbGuild.Members[victim.Id].InviteTracker.UserId))).AsError(ctx)));
                        return;
                    }

                    await this.ExecuteCommand(ctx, new Dictionary<string, object>
                    {
                        { "victim", newVictim }
                    });

                    return;
                }).Add(ctx.Bot, ctx);
            }
        });
    }
}
