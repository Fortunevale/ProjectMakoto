namespace ProjectIchigo.Commands;

internal class UrlSubmitCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string url = (string)arguments["url"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            var interactivity = ctx.Client.GetInteractivity();

            int tos_version = 2;

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != tos_version)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = $"{1.DigitsToEmotes()}. You may not submit URLs that are non-malicious.\n" +
                                  $"{2.DigitsToEmotes()}. You may not spam submissions.\n" +
                                  $"{3.DigitsToEmotes()}. You may not submit unregistered domains.\n" +
                                  $"{4.DigitsToEmotes()}. You may not submit shortened URLs.\n" +
                                  $"{5.DigitsToEmotes()}. You accept that your user account and current server will be tracked and visible to verifiers.\n\n" +
                                  $"We reserve the right to ban you for any reason that may not be listed.\n" +
                                  $"**Failing to follow these conditions may get you or your guild blacklisted from using this bot.**\n" +
                                  $"**This includes, but is not limited to, pre-existing guilds with your ownership and future guilds.**\n\n" +
                                  $"To accept these conditions, please click the button below. If you do not see a button to interact with, update your discord client."
                }.SetAwaitingInput(ctx, "Phishing Link Submission");

                if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != 0 && ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, "The submission conditions have changed since you last accepted them. Please re-read them and agree to the new condiditions to continue.\n\n");
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await interactivity.WaitForButtonAsync(ctx.ResponseMessage, ctx.User, TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS = tos_version;

                var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed.SetSuccess(ctx, "Phishing Link Submission").WithDescription($"Continuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(2))}..")).AddComponents(accepted_button));

                await Task.Delay(2000);
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Processing your request..`"
            }.SetLoading(ctx, "Phishing Link Submission");

            await RespondOrEdit(embed);

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                embed.Description = $"`You cannot submit a domain for the next {ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                _ = RespondOrEdit(embed.SetError(ctx, "Phishing Link Submission"));
                return;
            }

            if (ctx.Bot.submittedUrls.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                if (ctx.Bot.submittedUrls.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 10)
                {
                    embed.Description = $"`You have 10 open url submissions. Please wait before trying to submit another url.`";
                    _ = RespondOrEdit(embed.SetError(ctx, "Phishing Link Submission"));
                    return;
                }
            }

            if (ctx.Bot.phishingUrlSubmissionUserBans.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`You are banned from submitting URLs.`\n" +
                                    $"`Reason: {ctx.Bot.phishingUrlSubmissionUserBans[ctx.User.Id].Reason}`";
                _ = RespondOrEdit(embed.SetError(ctx, "Phishing Link Submission"));
                return;
            }

            if (ctx.Bot.phishingUrlSubmissionGuildBans.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`This guild is banned from submitting URLs.`\n" +
                                    $"`Reason: {ctx.Bot.phishingUrlSubmissionGuildBans[ctx.Guild.Id].Reason}`";
                _ = RespondOrEdit(embed.SetError(ctx, "Phishing Link Submission"));
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
                _ = RespondOrEdit(embed.SetError(ctx, "Phishing Link Submission"));
                return;
            }

            embed.Description = $"`You are about to submit the domain '{domain.SanitizeForCodeBlock()}'. Do you want to proceed?`";
            embed.SetAwaitingInput(ctx, "Phishing Link Submission");

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit domain", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { MessageComponents.CancelButton }
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
                embed.SetLoading(ctx, "Phishing Link Submission");
                await RespondOrEdit(embed);

                embed.Description = $"`Checking if your domain is already in the database..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.phishingUrls)
                {
                    if (domain.Contains(b.Key))
                    {
                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') is already present in the database. Thanks for trying to contribute regardless.`";
                        embed.SetError(ctx, "Phishing Link Submission");
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Checking if your domain has already been submitted before..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.submittedUrls)
                {
                    if (b.Value.Url == domain)
                    {
                        embed.Description = $"`The domain ('{domain.SanitizeForCodeBlock()}') has already been submitted. Thanks for trying to contribute regardless.`";
                        embed.SetError(ctx, "Phishing Link Submission");
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Creating submission..`";
                await RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.UrlSubmissionsChannelId);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = StatusIndicatorIcons.Success, Name = $"Phishing Link Submission" },
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

                ctx.Bot.submittedUrls.Add(subbmited_msg.Id, new SubmittedUrlEntry
                {
                    Url = domain,
                    Submitter = ctx.User.Id,
                    GuildOrigin = ctx.Guild.Id
                });

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                embed.Description = $"`Submission created. Thanks for your contribution.`";
                embed.SetSuccess(ctx, "Phishing Link Submission");
                await RespondOrEdit(embed);
            }
            else if (e.Result.Interaction.Data.CustomId == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}