namespace ProjectIchigo.Commands;

internal class UrlSubmitCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string url = (string)arguments["url"];

            if (await ctx.Bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if (!ctx.Bot._users.List.ContainsKey(ctx.User.Id))
                ctx.Bot._users.List.Add(ctx.User.Id, new Users.Info(ctx.Bot));

            var interactivity = ctx.Client.GetInteractivity();

            if (!ctx.Bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                    Color = EmbedColors.Important,
                    Footer = ctx.GenerateUsedByFooter(),
                    Timestamp = DateTime.UtcNow,
                    Description = $"{1.DigitsToEmotes()}. You may not submit URLs that are non-malicous.\n" +
                                  $"{2.DigitsToEmotes()}. You may not spam submissions.\n" +
                                  $"{3.DigitsToEmotes()}. You may not submit unregistered domains.\n" +
                                  $"{4.DigitsToEmotes()}. You may not submit shortened URLs.\n\n" +
                                  $"We reserve the right to ban you for any reason that may not be listed.\n" +
                                  $"**Failing to follow these conditions may get you or your guild blacklisted from using this bot.**\n" +
                                  $"**This includes, but is not limited to, pre-existing guilds with your ownership and future guilds.**\n\n" +
                                  $"To accept these conditions, please click the button below. If you do not see a button to interact with, update your discord client."
                };

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await interactivity.WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot._users.List[ctx.User.Id].UrlSubmissions.AcceptedTOS = true;

                var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed.WithColor(EmbedColors.Success).WithDescription($"Continuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(2))}..")).AddComponents(accepted_button));

                await Task.Delay(2000);
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Phishing Link Submission • {ctx.Guild.Name}" },
                Color = EmbedColors.Processing,
                Footer = ctx.GenerateUsedByFooter(),
                Timestamp = DateTime.UtcNow,
                Description = $"`Processing your request..`"
            };

            await RespondOrEdit(embed);

            if (ctx.Bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot._status))
            {
                embed.Description = $"`You cannot submit a domain for the next {ctx.Bot._users.List[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = RespondOrEdit(embed.Build());
                return;
            }

            if (ctx.Bot._submittedUrls.List.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot._status))
            {
                if (ctx.Bot._submittedUrls.List.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 10)
                {
                    embed.Description = $"`You have 10 open url submissions. Please wait before trying to submit another url.`";
                    embed.Color = EmbedColors.Error;
                    embed.Author.IconUrl = Resources.LogIcons.Error;
                    _ = RespondOrEdit(embed.Build());
                    return;
                }
            }

            if (ctx.Bot._submissionBans.Users.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`You are banned from submitting URLs.`\n" +
                                    $"`Reason: {ctx.Bot._submissionBans.Users[ctx.User.Id].Reason}`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = RespondOrEdit(embed.Build());
                return;
            }

            if (ctx.Bot._submissionBans.Guilds.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                    $"`Reason: {ctx.Bot._submissionBans.Guilds[ctx.Guild.Id].Reason}`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = RespondOrEdit(embed.Build());
                return;
            }

            string domain = url.ToLower();

            if (domain.StartsWith("https://") || domain.StartsWith("http://"))
                domain = domain.Replace("https://", "").Replace("http://", "");

            if (domain.Contains('/'))
                domain = domain.Remove(domain.IndexOf("/"), domain.Length - domain.IndexOf("/"));

            if (!domain.Contains('.') || domain.Contains(' '))
            {
                embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') you're trying to submit is invalid.`";
                embed.Color = EmbedColors.Error;
                embed.Author.IconUrl = Resources.LogIcons.Error;
                _ = RespondOrEdit(embed.Build());
                return;
            }

            embed.Description = $"`You are about to submit the domain '{domain.SanitizeForCodeBlock()}'. When submitting, your public user account and guild will be tracked and visible to verifiers. Do you want to proceed?`";
            embed.Color = EmbedColors.Success;
            embed.Author.IconUrl = Resources.LogIcons.Info;

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit domain", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { Resources.CancelButton }
            }));

            var e = await interactivity.WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            await e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.Result.Interaction.Data.CustomId == ContinueButton.CustomId)
            {
                embed.Description = $"`Submitting your domain..`";
                embed.Color = EmbedColors.Loading;
                embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                await RespondOrEdit(embed);

                embed.Description = $"`Checking if your domain is already in the database..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot._phishingUrls.List)
                {
                    if (domain.Contains(b.Key))
                    {
                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') is already present in the database. Thanks for trying to contribute regardless.`";
                        embed.Color = EmbedColors.Error;
                        embed.Author.IconUrl = Resources.LogIcons.Error;
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Checking if your domain has already been submitted before..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot._submittedUrls.List)
                {
                    if (b.Value.Url == domain)
                    {
                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') has already been submitted. Thanks for trying to contribute regardless.`";
                        embed.Color = EmbedColors.Error;
                        embed.Author.IconUrl = Resources.LogIcons.Error;
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Creating submission..`";
                await RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.UrlSubmissionsChannelId);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 939750475354472478)));

                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Info, Name = $"Phishing Link Submission" },
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Submitted Url`: `{domain.SanitizeForCodeBlock()}`\n" +
                                    $"`Submission by`: `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n" +
                                    $"`Submitted on `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
                })
                .AddComponents(new List<DiscordComponent>
                {
                    { AcceptSubmission },
                    { DenySubmission },
                    { BanUserButton },
                    { BanGuildButton },
                }));

                ctx.Bot._submittedUrls.List.Add(subbmited_msg.Id, new SubmittedUrls.UrlInfo
                {
                    Url = domain,
                    Submitter = ctx.User.Id,
                    GuildOrigin = ctx.Guild.Id
                });

                ctx.Bot._users.List[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                embed.Description = $"`Submission created. Thanks for your contribution.`";
                embed.Color = EmbedColors.Success;
                embed.Author.IconUrl = Resources.LogIcons.Info;
                await RespondOrEdit(embed);
            }
            else if (e.Result.Interaction.Data.CustomId == Resources.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}