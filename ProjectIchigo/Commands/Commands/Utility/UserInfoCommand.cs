﻿namespace ProjectIchigo.Commands;

internal class UserInfoCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            DiscordUser victim = (DiscordUser)arguments["victim"];

            if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForLight(ctx.Client, ctx))
                return;

            if (!ctx.Bot._guilds[ctx.Guild.Id].Members.ContainsKey(victim.Id))
                ctx.Bot._guilds[ctx.Guild.Id].Members.Add(victim.Id, new(ctx.Bot._guilds[ctx.Guild.Id], victim.Id));

            if (victim is null)
                victim = ctx.User;

            victim = await victim.GetFromApiAsync();

            DiscordMember? bMember = null;

            try
            {
                bMember = await ctx.Guild.GetMemberAsync(victim.Id);
            }
            catch { }

            string GetStatusIcon(UserStatus? status)
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
                if (ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Count > 0)
                    GenerateRoles = string.Join(", ", ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].MemberRoles.Where(x => ctx.Guild.Roles.ContainsKey(x.Id)).Select(x => $"{ctx.Guild.GetRole(x.Id).Mention}"));
                else
                    GenerateRoles = "`User doesn't have any stored roles.`";
            }

            var banList = await ctx.Guild.GetBansAsync();
            bool isBanned = banList.Any(x => x.User.Id == victim.Id);
            DiscordBan? banDetails = (isBanned ? banList.First(x => x.User.Id == victim.Id) : null);

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
                Description = $"{(bMember is null ? $"{(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate == DateTime.UnixEpoch ? "`User never joined this server.`" : $"{(isBanned ? "`User is currently banned from this server.`" : "`User is currently not in this server.`")}")}\n\n" : "")}" +
                        $"{(bMember is not null && bMember.IsOwner ? "✨ `This user owns this guild`\n" : "")}" +
                        $"{(victim.IsStaff ? "📘 `Discord Staff`\n" : "")}" +
                        $"{(victim.IsMod ? "⚒ `Certified Content Moderator`\n" : "")}" +
                        $"{(victim.IsBotDev ? "⌨ `Verified Bot Developer`\n" : "")}" +
                        $"{(victim.IsPartner ? "👥 `Discord Partner`\n" : "")}" +
                        $"{(bMember is not null && bMember.IsPending.HasValue && bMember.IsPending.Value ? "❗ `User's Membership pending`\n" : "")}" +
                        $"\n**{(bMember is null ? "Roles (Backup)" : "Roles")}**\n{GenerateRoles}"
            };

            if (isBanned)
                embed.AddField(new DiscordEmbedField("Ban Details", $"`{(string.IsNullOrWhiteSpace(banDetails?.Reason) ? "No reason provided." : $"{banDetails.Reason}")}`", false));

            if (ctx.Bot._guilds[ctx.Guild.Id].InviteTrackerSettings.Enabled)
            {
                embed.AddField(new DiscordEmbedField("Invited by", $"{(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.Code.IsNullOrWhiteSpace() ? "`No inviter found.`" : $"<@{ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}> (`{ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].InviteTracker.UserId}`)")}", true));
                embed.AddField(new DiscordEmbedField("Users invited", $"`{(ctx.Bot._guilds[ctx.Guild.Id].Members.Where(b => b.Value.InviteTracker.UserId == victim.Id)).Count()}`", true));
            }

            if (bMember is not null)
                embed.AddField(new DiscordEmbedField("Server Join Date", $"{Formatter.Timestamp(bMember.JoinedAt, TimestampFormat.LongDateTime)}", true));
            else
                embed.AddField(new DiscordEmbedField("Server Leave Date", (ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].LastLeaveDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField("First Join Date", (ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate != DateTime.UnixEpoch ? $"{Formatter.Timestamp(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate, TimestampFormat.LongDateTime)} ({Formatter.Timestamp(ctx.Bot._guilds[ctx.Guild.Id].Members[victim.Id].FirstJoinDate)})" : "`User never joined this server.`"), true));

            embed.AddField(new DiscordEmbedField("Account Creation Date", $"{Formatter.Timestamp(victim.CreationTimestamp, TimestampFormat.LongDateTime)}", true));

            if (bMember is not null && bMember.PremiumSince.HasValue)
                embed.AddField(new DiscordEmbedField("Server Booster Since", $"{Formatter.Timestamp(bMember.PremiumSince.Value, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Pronouns))
                embed.AddField(new DiscordEmbedField("Pronouns", $"`{victim.Pronouns}`", true));

            if (victim.BannerColor is not null)
                embed.AddField(new DiscordEmbedField("Banner Color", $"`{victim.BannerColor.Value}`", true));

            if (victim.Presence is not null)
                embed.AddField(new DiscordEmbedField("Current Presence", $"{GetStatusIcon(victim.Presence.Status)} `{victim.Presence.Status}`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Desktop.HasValue ? victim.Presence.ClientStatus.Desktop.Value : UserStatus.Offline)} `Desktop`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Mobile.HasValue ? victim.Presence.ClientStatus.Mobile.Value : UserStatus.Offline)} `Mobile`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Web.HasValue ? victim.Presence.ClientStatus.Web.Value : UserStatus.Offline)} `Web`\n\n", true));

            try
            {
                if (victim.Presence is not null && victim.Presence.Activities is not null && victim.Presence.Activities?.Count > 0)
                    embed.AddField(new DiscordEmbedField("Current Activities", string.Join("\n", victim.Presence.Activities.Select(x => $"{(x.ActivityType == ActivityType.Custom ? $"• Status: `{x.CustomStatus.Emoji.Name}`{(string.IsNullOrWhiteSpace(x.CustomStatus.Name) ? "" : $" {x.CustomStatus.Name}")}\n" : $"• {x.ActivityType} {x.Name}")}")), true));
            }
            catch { }

            if (bMember is not null && bMember.CommunicationDisabledUntil.HasValue)
                embed.AddField(new DiscordEmbedField("Timed out until", $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value, TimestampFormat.LongDateTime)}", true));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        });
    }

}
