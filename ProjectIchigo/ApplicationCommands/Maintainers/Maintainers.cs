using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;

namespace ProjectIchigo.ApplicationCommands.Maintainers;

internal class Maintainers : ApplicationCommandsModule
{
    public Bot _bot { private get; set; }

    [SlashCommandGroup("github", "Interact with Project-Ichigo's Github Repository")]
    public class Github : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("create-issue", "Create a new issue on Project-Ichigo's Github Repository")]
        public async Task CreateIssue(InteractionContext ctx, [Option("use_old_tag_selector", "Allows the use of the legacy tag selector.")]bool UseOldTagsSelector = false)
        {
            Task.Run(async () =>
            { 
                if (!ctx.User.IsMaintenance(_bot._status))
                {
                    _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ `This command is restricted to Staff Members of Project Ichigo.`"));
                    return;
                }

                if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                {
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent($"❌ `The GitHub Token expired, please update.`"));
                    return;
                }

                var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

                var tokenAuth = new Credentials(Secrets.Secrets.GithubToken);
                client.Credentials = tokenAuth;

                var labels = await client.Issue.Labels.GetAllForRepository(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository);

                var modal = new DiscordInteractionModalBuilder().WithCustomId(Guid.NewGuid().ToString()).WithTitle("Create new Issue on Github")
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "New issue", 4, 250, true))
                    .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description", "Description", required: false));

                if (!UseOldTagsSelector)
                    modal.AddModalComponents(new DiscordSelectComponent("Select tags", labels.Select(x => new DiscordSelectComponentOption(x.Name, x.Name.ToLower().MakeValidFileName(), "", false, new DiscordComponentEmoji(new DiscordColor(x.Color).GetClosestColorEmoji(ctx.Client)))), "labels", 1, labels.Count));
                else
                    modal.AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "labels", "Labels", "", null, null, false, $"Put a # in front of every label you want to add.\n\n{string.Join("\n", labels.Select(x => x.Name))}"));

                await ctx.CreateModalResponseAsync(modal);

                CancellationTokenSource cancellationTokenSource = new();

                ctx.Client.ComponentInteractionCreated += RunInteraction;

                async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
                {
                    Task.Run(async () =>
                    {
                        if (e.Interaction.Data.CustomId == modal.CustomId)
                        {
                            cancellationTokenSource.Cancel();
                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            var followup = await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { IsEphemeral = true }.WithContent(":arrows_counterclockwise: `Submitting your issue..`"));

                            var labelComp = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "labels").First().Components.First();

                            string title = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "title").First().Components.First().Value;
                            string description = e.Interaction.Data.Components.Where(x => x.Components.First().CustomId == "description").First().Components.First().Value;
                            List<string> labels;

                            if (labelComp.Type == ComponentType.Select)
                            {
                                labels = labelComp.Values.ToList();
                            }
                            else
                            {
                                labels = labelComp.Value.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith("#")).Select(x => x.Replace("#", "")).ToList();
                            }

                            if (Secrets.Secrets.GithubTokenExperiation.GetTotalSecondsUntil() <= 0)
                            {
                                _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"❌ `The GitHub Token expired, please update.`"));
                                return;
                            }

                            var issue = await client.Issue.Create(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository, new NewIssue(title) { Body = description });

                            if (labels.Count > 0)
                                await client.Issue.Labels.ReplaceAllForIssue(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository, issue.Number, labels.ToArray());

                            await client.Issue.Assignee.AddAssignees(Secrets.Secrets.GithubUsername, Secrets.Secrets.GithubRepository, issue.Number, new AssigneesUpdate(new List<string> { Secrets.Secrets.GithubUsername }));

                            _ = e.Interaction.EditFollowupMessageAsync(followup.Id, new DiscordWebhookBuilder().WithContent($"✅ `Issue submitted:` {issue.HtmlUrl}"));
                        }
                    }).Add(_bot._watcher, ctx);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(15), cancellationTokenSource.Token);

                    ctx.Client.ComponentInteractionCreated -= RunInteraction;
                }
                catch { }
            }).Add(_bot._watcher, ctx);
        }
    }

    [SlashCommandGroup("debug", "Debug commands")]
    public class Debug : ApplicationCommandsModule
    {
        public Bot _bot { private get; set; }

        [SlashCommand("throw", "Throw.")]
        public async Task Throw(InteractionContext ctx)
        {
            Task.Run(async () =>
            {
                if (!ctx.User.IsMaintenance(_bot._status))
                {
                    _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ `This command is restricted to Staff Members of Project Ichigo.`"));
                    return;
                }

                throw new InvalidCastException();
            }).Add(_bot._watcher, ctx);
        }
    }

    [SlashCommand("detailed_userinfo", "View discord user information.")]
    public async Task DiscordLookup(InteractionContext ctx, [Option("User", "The user or user id.")] DiscordUser victim)
    {
        Task.Run(async () =>
        {
            if (!ctx.User.IsMaintenance(_bot._status))
            {
                _ = ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"❌ `This command is restricted to Staff Members of Project Ichigo.`"));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"Fetching user information for {victim.Mention}.."));

            DiscordMember? bMember = null;

            try
            {
                bMember = await ctx.Guild.GetMemberAsync(victim.Id);
            }
            catch { }

            string GetNitroText(PremiumType? type)
            {
                return type switch
                {
                    PremiumType.NitroClassic => $"💵 `Nitro Classic`\n",
                    PremiumType.Nitro => "💵 `Nitro`\n",
                    PremiumType.NitroLite => "💵 `Nitro Lite`\n",
                    _ => "",
                };
            }

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
                Description = $"{GetNitroText(victim.PremiumType)}" +
                        $"{(bMember is not null && bMember.IsOwner ? "✨ `This user owns this guild`\n" : "")}" +
                        $"{(victim.IsCurrent ? "⚙ `Currently running with this account`\n" : "")}" +
                        $"{(victim.IsStaff ? "📘 `Discord Staff`\n" : "")}" +
                        $"{(victim.IsMod ? "⚒ `Certified Content Moderator`\n" : "")}" +
                        $"{(victim.IsBotDev ? "⌨ `Verified Bot Developer`\n" : "")}" +
                        $"{(victim.IsPartner ? "👥 `Discord Partner`\n" : "")}" +
                        $"{(victim.Verified ?? false ? "✅ `Verified E-Mail Address`\n" : "")}" +
                        $"{(victim.MfaEnabled ?? false ? "🔐 `Multi Factor Authentication enabled`\n" : "")}" +
                        $"{(bMember is not null && bMember.IsPending.HasValue && bMember.IsPending.Value ? "❗ `User's Membership pending`\n" : "")}" +
                        $"{(victim.Flags.HasValue ? $"\n**User Flags**\n{string.Join(", ", victim.Flags.Value.ToString().Split(", ").Select(x => $"`{x}`"))}\n" : "")}" +
                        $"{(bMember is not null && bMember.MemberFlags != MemberFlags.None ? $"\n**Member Flags**\n{string.Join(", ", bMember.MemberFlags.ToString().Split(", ").Select(x => $"`{x}`"))}\n" : "")}" +
                        $"{(victim.OAuthFlags.HasValue && victim.OAuthFlags.Value != UserFlags.None ? $"\n**OAuth Flags**\n{string.Join(", ", victim.OAuthFlags.Value.ToString().Split(", ").Select(x => $"`{x}`"))}\n" : "")}" +
                        $"\n**Roles**\n{(bMember?.Roles.Count() > 0 ? string.Join(", ", bMember.Roles.Select(x => x.Mention)) : $"`The user doesn't have any roles on this server.`")}"
            };

            var banList = await ctx.Guild.GetBansAsync();
            bool isBanned = banList.Any(x => x.User.Id == victim.Id);
            DiscordBan? banDetails = (isBanned ? banList.First(x => x.User.Id == victim.Id) : null);

            if (isBanned)
                embed.AddField(new DiscordEmbedField("Ban Details", $"`{(string.IsNullOrWhiteSpace(banDetails?.Reason) ? "No reason provided." : $"{banDetails.Reason}")}`", false));

            if (bMember is not null && !string.IsNullOrWhiteSpace(bMember.Nickname))
                embed.AddField(new DiscordEmbedField("Nickname", $"`{bMember.Nickname}`", true));

            embed.AddField(new DiscordEmbedField("Creation Date", $"{Formatter.Timestamp(victim.CreationTimestamp, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Email))
                embed.AddField(new DiscordEmbedField("E-Mail", $"`{victim.Email}`", true));

            if (bMember is not null && bMember.PremiumSince.HasValue)
                embed.AddField(new DiscordEmbedField("Premium Since", $"{Formatter.Timestamp(bMember.PremiumSince.Value, TimestampFormat.LongDateTime)}", true));

            if (bMember is not null)
                embed.AddField(new DiscordEmbedField("Guild Join Date", $"{Formatter.Timestamp(bMember.JoinedAt, TimestampFormat.LongDateTime)}", true));

            if (!string.IsNullOrWhiteSpace(victim.Pronouns))
                embed.AddField(new DiscordEmbedField("Pronouns", $"`{victim.Pronouns}`", true));

            if (!string.IsNullOrWhiteSpace(victim.AvatarHash))
                embed.AddField(new DiscordEmbedField("Avatar Hash", $"`{victim.AvatarHash}`", true));

            if (!string.IsNullOrWhiteSpace(victim.Locale))
                embed.AddField(new DiscordEmbedField("Locale", $"`{victim.Locale}`", true));

            if (victim.BannerColor is not null)
                embed.AddField(new DiscordEmbedField("Banner Color", $"`{victim.BannerColor.Value}`", true));

            if (!string.IsNullOrWhiteSpace(victim.BannerUrl))
                embed.AddField(new DiscordEmbedField("Banner Url", $"{victim.BannerUrl}", true));

            if (!string.IsNullOrWhiteSpace(victim.BannerHash))
                embed.AddField(new DiscordEmbedField("Banner Hash", $"`{victim.BannerHash}`", true));

            if (bMember is not null && !string.IsNullOrWhiteSpace(bMember.GuildAvatarHash))
            {
                embed.AddField(new DiscordEmbedField("Guild Avatar Url", $"[Open in browser]({bMember.GuildAvatarUrl})", true));
                embed.AddField(new DiscordEmbedField("Guild Avatar Hash", $"`{bMember.GuildAvatarHash}`", true));
            }

            if (bMember is not null && !string.IsNullOrWhiteSpace(bMember.GuildBannerHash))
            {
                embed.AddField(new DiscordEmbedField("Guild Banner Url", $"[Open in browser]({bMember.GuildBannerUrl})", true));
                embed.AddField(new DiscordEmbedField("Guild Banner Hash", $"`{bMember.GuildBannerHash}`", true));
            }

            if (victim.Presence is not null)
                embed.AddField(new DiscordEmbedField("Presence", $"{GetStatusIcon(victim.Presence.Status)} `{victim.Presence.Status}`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Desktop.HasValue ? victim.Presence.ClientStatus.Desktop.Value : UserStatus.Offline)} `Desktop`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Mobile.HasValue ? victim.Presence.ClientStatus.Mobile.Value : UserStatus.Offline)} `Mobile`\n" +
                                                                $"󠂪 󠂪 󠂪 󠂪{GetStatusIcon(victim.Presence.ClientStatus.Web.HasValue ? victim.Presence.ClientStatus.Web.Value : UserStatus.Offline)} `Web`\n\n", true));

            if (victim.Presence is not null && victim.Presence.Activities?.Count > 0)
                embed.AddField(new DiscordEmbedField("Activities", string.Join("\n", victim.Presence.Activities.Select(x => $"{(x.ActivityType == ActivityType.Custom ? $"<:dot:984701737552187433> Status: {x.CustomStatus.Emoji}{(string.IsNullOrWhiteSpace(x.CustomStatus.Name) ? "" : $" {x.CustomStatus.Name}")}\n" : $"<:dot:984701737552187433> {x.ActivityType} {x.Name}")}")), true));

            if (bMember is not null && bMember.CommunicationDisabledUntil.HasValue)
                embed.AddField(new DiscordEmbedField("Communication disabled until", $"{Formatter.Timestamp(bMember.CommunicationDisabledUntil.Value, TimestampFormat.LongDateTime)}", true));

            await ctx.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }).Add(_bot._watcher, ctx);
    }
}
