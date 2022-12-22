namespace ProjectIchigo.Commands;

internal class UserInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
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
                    Name = $"{(victim.IsBot ? $"[{(victim.IsSystem ?? false ? "System" : $"Bot{(victim.IsVerifiedBot ? "✅" : "❎")}")}] " : "")}{victim.UsernameWithDiscriminator}",
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
                Description = $"{(bMember is null ? $"{(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate == DateTime.UnixEpoch ? "`User never joined this server.`" : $"{(isBanned ? "`User is currently banned from this server.`" : "`User is currently not in this server.`")}")}\n\n" : "")}" +
                        $"{(ctx.Bot.globalBans.ContainsKey(victim.Id) ? "💀 **`User is globally banned.`**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamOwner == victim.Id ? $"👑 **`{ctx.CurrentUser.Username} Owner`**\n" : "")}" +
                        $"{(ctx.Bot.status.TeamMembers.Contains(victim.Id) ? $"🔏 **`{ctx.CurrentUser.Username} Staff`**\n\n" : "")}" +
                        $"{(bMember is not null && bMember.IsOwner ? "✨ `This user owns this guild`\n" : "")}" +
                        $"{(victim.IsStaff ? "📘 **`Discord Staff`**\n" : "")}" +
                        $"{(victim.IsMod ? "⚒ `Certified Content Moderator`\n" : "")}" +
                        $"{(victim.IsBotDev ? "⌨ `Verified Bot Developer`\n" : "")}" +
                        $"{(victim.IsPartner ? "👥 `Discord Partner`\n" : "")}" +
                        $"{(bMember is not null && bMember.IsPending.HasValue && bMember.IsPending.Value ? "❗ `User's Membership pending`\n" : "")}" +
                        $"\n**{(bMember is null ? "Roles (Backup)" : "Roles")}**\n{GenerateRoles}"
            };

            if (ctx.Bot.globalNotes.ContainsKey(victim.Id) && ctx.Bot.globalNotes[victim.Id].Any())
            {
                embed.AddField(new DiscordEmbedField($"{ctx.CurrentUser.Username} Staff Notes", $"{string.Join("\n\n", ctx.Bot.globalNotes[victim.Id].Select(x => $"{x.Reason.FullSanitize()} - <@{x.Moderator}> {x.Timestamp.ToTimestamp()}"))}".TruncateWithIndication(512)));
            }

            if (ctx.Bot.globalBans.ContainsKey(victim.Id))
            {
                var gBanDetails = ctx.Bot.globalBans[victim.Id];
                var gBanMod = await ctx.Client.GetUserAsync(ctx.Bot.globalBans[victim.Id].Moderator);

                embed.AddField(new DiscordEmbedField("Global Ban Reason", $"`{((string.IsNullOrWhiteSpace(gBanDetails.Reason) || gBanDetails.Reason == "-") ? "No reason provided." : gBanDetails.Reason).SanitizeForCode()}`", true));
                embed.AddField(new DiscordEmbedField("Global Ban Moderator", $"`{gBanMod.UsernameWithDiscriminator}`", true));
                embed.AddField(new DiscordEmbedField("Global Ban Date", $"{Formatter.Timestamp(gBanDetails.Timestamp)} ({Formatter.Timestamp(gBanDetails.Timestamp, TimestampFormat.LongDateTime)})", true));
            }

            if (isBanned)
                embed.AddField(new DiscordEmbedField("Ban Details", $"`{(string.IsNullOrWhiteSpace(banDetails?.Reason) ? "No reason provided." : $"{banDetails.Reason}")}`", false));

            bool InviterButtonAdded = false;

            if (ctx.Bot.guilds[ctx.Guild.Id].InviteTracker.Enabled)
            {
                embed.AddField(new DiscordEmbedField("Invited by", $"{(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace() ? "`No inviter found.`" : $"<@{ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}> (`{ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}`)")}", true));
                embed.AddField(new DiscordEmbedField("Users invited", $"`{(ctx.Bot.guilds[ctx.Guild.Id].Members.Where(b => b.Value.InviteTracker.UserId == victim.Id)).Count()}`", true));

                if (!ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace())
                {
                    InviterButtonAdded = true;
                    builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Secondary, $"userinfo-inviter", "Show Profile of Inviter", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤"))));
                }
            }

            if (bMember is not null)
                embed.AddField(new DiscordEmbedField("Server Join Date", $"{Formatter.Timestamp(bMember.JoinedAt, TimestampFormat.LongDateTime)}", true));
            else
                embed.AddField(new DiscordEmbedField("Server Leave Date", (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField("First Join Date", (ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField("Account Creation Date", $"{Formatter.Timestamp(victim.CreationTimestamp, TimestampFormat.LongDateTime)}", true));

            if (bMember is not null && bMember.PremiumSince.HasValue)
                embed.AddField(new DiscordEmbedField("Server Booster Since", $"{Formatter.Timestamp(bMember.PremiumSince.Value, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Pronouns))
                embed.AddField(new DiscordEmbedField("Pronouns", $"`{victim.Pronouns}`", true));

            if (victim.BannerColor is not null)
                embed.AddField(new DiscordEmbedField("Banner Color", $"`{victim.BannerColor.Value}`", true));

            try
            {
                if (victim.Presence is not null)
                    embed.AddField(new DiscordEmbedField("Current Presence", $"{GetStatusIcon(victim.Presence.Status)} `{victim.Presence.Status}`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Desktop.HasValue ? victim.Presence.ClientStatus.Desktop.Value : UserStatus.Offline)} `Desktop`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Mobile.HasValue ? victim.Presence.ClientStatus.Mobile.Value : UserStatus.Offline)} `Mobile`\n" +
                                                                    $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Web.HasValue ? victim.Presence.ClientStatus.Web.Value : UserStatus.Offline)} `Web`\n\n", true));
            }
            catch { }

            try
            {
                if (victim.Presence is not null && victim.Presence.Activities is not null && victim.Presence.Activities?.Count > 0)
                    embed.AddField(new DiscordEmbedField("Current Activities", string.Join("\n", victim.Presence.Activities.Select(x => $"{(x.ActivityType == ActivityType.Custom ? $"• Status: `{x.CustomStatus.Emoji.Name}`{(string.IsNullOrWhiteSpace(x.CustomStatus.Name) ? "" : $" {x.CustomStatus.Name}")}\n" : $"• {x.ActivityType} {x.Name}")}")), true));
            }
            catch { }

            if (bMember is not null && bMember.CommunicationDisabledUntil.HasValue && bMember.CommunicationDisabledUntil.Value.GetTotalSecondsUntil() > 0)
                embed.AddField(new DiscordEmbedField("Timed out until", $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value, TimestampFormat.LongDateTime)}", true));

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
                            .AddEmbed(new DiscordEmbedBuilder().WithDescription($"`Failed to fetch user '{ctx.Bot.guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}'`").AsError(ctx)));
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
