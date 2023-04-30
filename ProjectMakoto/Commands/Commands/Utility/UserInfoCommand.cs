﻿namespace ProjectMakoto.Commands;

internal class UserInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx))
                return;

            victim ??= ctx.User;

            if (!ctx.Bot.guilds[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                ctx.Bot.guilds[ctx.Guild.Id].Members.Add(victim.Id, new(ctx.Bot.guilds[ctx.Guild.Id], victim.Id));

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

            string GenerateRoles = "";

            if (bMember is not null)
            {
                if (bMember.Roles.Any())
                    GenerateRoles = string.Join(", ", bMember.Roles.Select(x => x.Mention));
                else
                    GenerateRoles = "`User doesn't have any roles.`";
            }
            else
            {
                if (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Count > 0)
                    GenerateRoles = string.Join(", ", ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Where(x => ctx.Guild.Roles.ContainsKey(x.Id)).Select(x => $"{ctx.Guild.GetRole(x.Id).Mention}"));
                else
                    GenerateRoles = "`User doesn't have any stored roles.`";
            }

            var banList = await ctx.Guild.GetBansAsync();
            bool isBanned = banList.Any(x => x.User.Id == victim.Id);
            DiscordBan? banDetails = (isBanned ? banList.First(x => x.User.Id == victim.Id) : null);

            var builder = new DiscordMessageBuilder();

            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"{(victim.IsBot ? $"[{(victim.IsSystem ?? false ? GetString(t.Commands.Utility.UserInfo.System) : $"{GetString(t.Commands.Utility.UserInfo.Bot)}{(victim.IsVerifiedBot ? "✅" : "❎")}")}] " : "")}{victim.UsernameWithDiscriminator}",
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
                Description = $"{(bMember is null ? $"{(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate == DateTime.UnixEpoch ? $"`{GetString(t.Commands.Utility.UserInfo.NeverJoined)}`" : $"{(isBanned ? $"`{GetString(t.Commands.Utility.UserInfo.IsBanned)}`" : $"`{GetString(t.Commands.Utility.UserInfo.JoinedBefore)}`")}")}\n\n" : "")}" +
                        $"{(ctx.Bot.globalBans.ContainsKey(victim.Id) ? $"💀 **`{GetString(t.Commands.Utility.UserInfo.GlobalBanned)}`**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamOwner == victim.Id ? $"👑 **`{GetString(t.Commands.Utility.UserInfo.BotOwner).Replace("{Bot}", ctx.CurrentUser.Username)}`**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamMembers.Contains(victim.Id) ? $"🔏 **`{GetString(t.Commands.Utility.UserInfo.BotStaff).Replace("{Bot}", ctx.CurrentUser.Username)}`**\n\n" : "")}" +
                        $"{(bMember is not null && bMember.IsOwner ? $"✨ `{GetString(t.Commands.Utility.UserInfo.Owner)}`\n" : "")}" +
                        $"{(victim.IsStaff ? $"📘 **`{GetString(t.Commands.Utility.UserInfo.DiscordStaff)}`**\n" : "")}" +
                        $"{(victim.IsMod ? $"⚒ `{GetString(t.Commands.Utility.UserInfo.CertifiedMod)}`\n" : "")}" +
                        $"{(victim.IsBotDev ? $"⌨ `{GetString(t.Commands.Utility.UserInfo.VerifiedBotDeveloper)}`\n" : "")}" +
                        $"{(victim.IsPartner ? $"👥 `{GetString(t.Commands.Utility.UserInfo.DiscordPartner)}`\n" : "")}" +
                        $"{(bMember is not null && bMember.IsPending.HasValue && bMember.IsPending.Value ? $"❗ `{GetString(t.Commands.Utility.UserInfo.PendingMembership)}`\n" : "")}" +
                        $"\n**{(bMember is null ? $"{GetString(t.Commands.Utility.UserInfo.Roles)} ({GetString(t.Commands.Utility.UserInfo.Backup)})" : GetString(t.Commands.Utility.UserInfo.Roles))}**\n{GenerateRoles}"
            };

            if (ctx.Bot.globalNotes.TryGetValue(victim.Id, out List<GlobalBanDetails> globalNotes) && globalNotes.Any())
            {
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.BotNotes).Replace("{Bot}", ctx.CurrentUser.Username), $"{string.Join("\n\n", ctx.Bot.globalNotes[victim.Id].Select(x => $"{x.Reason.FullSanitize()} - <@{x.Moderator}> {x.Timestamp.ToTimestamp()}"))}".TruncateWithIndication(512)));
            }

            if (ctx.Bot.globalBans.TryGetValue(victim.Id, out GlobalBanDetails globalBanDetails))
            {
                var gBanMod = await ctx.Client.GetUserAsync(ctx.Bot.globalBans[victim.Id].Moderator);

                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.GlobalBanReason), $"`{((string.IsNullOrWhiteSpace(globalBanDetails.Reason) || globalBanDetails.Reason == "-") ? GetString(t.Commands.Utility.UserInfo.NoReason) : globalBanDetails.Reason).SanitizeForCode()}`", true));
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.GlobalBanMod), $"`{gBanMod.UsernameWithDiscriminator}`", true));
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.GlobalBanDate), $"{Formatter.Timestamp(globalBanDetails.Timestamp)} ({Formatter.Timestamp(globalBanDetails.Timestamp, TimestampFormat.LongDateTime)})", true));
            }

            if (isBanned)
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.BanDetails), $"`{(string.IsNullOrWhiteSpace(banDetails?.Reason) ? GetString(t.Commands.Utility.UserInfo.NoReason) : $"{banDetails.Reason}")}`", false));

            bool InviterButtonAdded = false;

            if (ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled)
            {
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.InvitedBy), $"{(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace() ? $"`{GetString(t.Commands.Utility.UserInfo.NoInviter)}`" : $"<@{ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}> (`{ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}`)")}", true));
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.UsersInvited), $"`{(ctx.Bot.guilds[ctx.Guild.Id].Members.Where(b => b.Value.InviteTracker.UserId == victim.Id)).Count()}`", true));

                if (!ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace())
                {
                    InviterButtonAdded = true;
                    builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, $"userinfo-inviter", GetString(t.Commands.Utility.UserInfo.ShowProfileInviter), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤"))));
                }
            }

            if (bMember is not null)
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.ServerJoinDate), $"{Formatter.Timestamp(bMember.JoinedAt, TimestampFormat.LongDateTime)}", true));
            else
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.ServerLeaveDate), (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.FirstJoinDate), (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.AccountCreationDate), $"{Formatter.Timestamp(victim.CreationTimestamp, TimestampFormat.LongDateTime)}", true));

            if (bMember is not null && bMember.PremiumSince.HasValue)
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.ServerBoosterSince), $"{Formatter.Timestamp(bMember.PremiumSince.Value, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Pronouns))
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.Pronouns), $"`{victim.Pronouns}`", true));

            if (victim.BannerColor is not null)
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.BannerColor), $"`{victim.BannerColor.Value}`", true));

            string TranslatePresence(UserStatus status)
            {
                return status switch
                {
                    UserStatus.Online => GetString(t.Commands.Utility.UserInfo.Online),
                    UserStatus.Idle => GetString(t.Commands.Utility.UserInfo.Idle),
                    UserStatus.DoNotDisturb => GetString(t.Commands.Utility.UserInfo.DoNotDisturb),
                    UserStatus.Streaming => GetString(t.Commands.Utility.UserInfo.Streaming),
                    UserStatus.Offline => GetString(t.Commands.Utility.UserInfo.Offline),
                    UserStatus.Invisible => GetString(t.Commands.Utility.UserInfo.Offline),
                    _ => status.ToString(),
                };
            }

            try
            {
                if (victim.Presence is not null)
                    embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.Presence), $"{GetStatusIcon(victim.Presence.Status)} `{TranslatePresence(victim.Presence.Status)}`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Desktop.HasValue ? victim.Presence.ClientStatus.Desktop.Value : UserStatus.Offline)} `{GetString(t.Commands.Utility.UserInfo.Desktop)}`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Mobile.HasValue ? victim.Presence.ClientStatus.Mobile.Value : UserStatus.Offline)} `{GetString(t.Commands.Utility.UserInfo.Mobile)}`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Web.HasValue ? victim.Presence.ClientStatus.Web.Value : UserStatus.Offline)} `{GetString(t.Commands.Utility.UserInfo.Web)}`\n\n", true));
            }
            catch { }

            string TranslateActivity(ActivityType type)
            {
                return type switch
                {
                    ActivityType.Playing => GetString(t.Commands.Utility.UserInfo.Playing),
                    ActivityType.Streaming => GetString(t.Commands.Utility.UserInfo.Streaming),
                    ActivityType.ListeningTo => GetString(t.Commands.Utility.UserInfo.ListeningTo),
                    ActivityType.Watching => GetString(t.Commands.Utility.UserInfo.Watching),
                    ActivityType.Competing => GetString(t.Commands.Utility.UserInfo.Competing),
                    _ => type.ToString(),
                };
            }

            try
            {
                if (victim.Presence is not null && victim.Presence.Activities is not null && victim.Presence.Activities?.Count > 0)
                    embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.Activities), string.Join("\n", victim.Presence.Activities.Select(x => $"{(x.ActivityType == ActivityType.Custom ? $"• {GetString(t.Commands.Utility.UserInfo.Status)}: `{x.CustomStatus.Emoji.Name}`{(string.IsNullOrWhiteSpace(x.CustomStatus.Name) ? "" : $" {x.CustomStatus.Name}")}\n" : $"• {TranslateActivity(x.ActivityType)} {x.Name}")}")), true));
            }
            catch { }

            if (bMember is not null && bMember.CommunicationDisabledUntil.HasValue && bMember.CommunicationDisabledUntil.Value.GetTotalSecondsUntil() > 0)
                embed.AddField(new DiscordEmbedField(GetString(t.Commands.Utility.UserInfo.TimedOutUntil), $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value, TimestampFormat.LongDateTime)}", true));

            await RespondOrEdit(builder.WithEmbed(embed));

            if (InviterButtonAdded)
            {
                ctx.ResponseMessage.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(15)).ContinueWith(async x =>
                {
                    if (x.IsFaulted)
                        return;

                    var e = x.Result;

                    if (e.TimedOut)
                    {
                        ModifyToTimedOut();
                        return;
                    }

                    _ = e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                    DiscordUser newVictim;

                    try
                    {
                        newVictim = await ctx.Client.GetUserAsync(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId);
                    }
                    catch (Exception)
                    {
                        _ = e.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                            .AddEmbed(new DiscordEmbedBuilder().WithDescription($"`{GetString(t.Commands.Utility.UserInfo.FetchUserError).Replace("{User}", ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId)}`").AsError(ctx)));
                        return;
                    }

                    await ExecuteCommand(ctx, new Dictionary<string, object>
                    {
                        { "victim", newVictim }
                    });

                    return;
                }).Add(ctx.Bot.watcher, ctx);
            }
        });
    }

}
