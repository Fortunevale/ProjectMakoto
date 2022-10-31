namespace ProjectIchigo.Commands;

internal class ReportHostCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            string url = (string)arguments["url"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            int tos_version = 3;

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != tos_version)
            {
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", "I accept these conditions", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = $"{1.ToEmotes()}. You may not submit Hosts that are non-malicious.\n" +
                                  $"{2.ToEmotes()}. You may not spam submissions.\n" +
                                  $"{3.ToEmotes()}. You may not submit unregistered hosts.\n" +
                                  $"{4.ToEmotes()}. You accept that your user account and current server will be tracked and visible to Ichigo staff.\n\n" +
                                  $"We reserve the right to ban you for any reason that may not be listed.\n" +
                                  $"**Failing to follow these conditions may get you or your guild blacklisted from using this bot.**\n" +
                                  $"**This includes, but is not limited to, pre-existing guilds with your ownership and future guilds.**\n\n" +
                                  $"To accept these conditions, please click the button below. If you do not see a button, update your discord client."
                }.AsAwaitingInput(ctx, "Malicious Host Submissions");

                if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != 0 && ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, "**The submission conditions have changed since you last accepted them. Please re-read them and agree to the new condiditions to continue.**\n\n");
                }

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed).AddComponents(button));

                var TosAccept = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

                if (TosAccept.TimedOut)
                {
                    ModifyToTimedOut(true);
                    return;
                }

                await TosAccept.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS = tos_version;

                var accepted_button = new DiscordButtonComponent(ButtonStyle.Success, "no_id", "Conditions accepted", true, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(tos_embed.AsSuccess(ctx, "Malicious Host Submissions").WithDescription($"Continuing {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(2))}..")).AddComponents(accepted_button));

                await Task.Delay(2000);
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Processing your request..`"
            }.AsLoading(ctx, "Malicious Host Submissions");

            await RespondOrEdit(embed);

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                embed.Description = $"`You cannot submit a host for the next {ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).GetTimespanUntil().GetHumanReadable()}.`";
                _ = RespondOrEdit(embed.AsError(ctx, "Malicious Host Submissions"));
                return;
            }

            if (ctx.Bot.submittedUrls.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                if (ctx.Bot.submittedUrls.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 5)
                {
                    embed.Description = $"`You have 5 open host submissions. Please wait before trying to submit another host.`";
                    _ = RespondOrEdit(embed.AsError(ctx, "Malicious Host Submissions"));
                    return;
                }
            }

            if (ctx.Bot.phishingUrlSubmissionUserBans.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`You are banned from submitting hosts.`\n" +
                                    $"`Reason: {ctx.Bot.phishingUrlSubmissionUserBans[ctx.User.Id].Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, "Malicious Host Submissions"));
                return;
            }

            if (ctx.Bot.phishingUrlSubmissionGuildBans.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`This guild is banned from submitting hosts.`\n" +
                                    $"`Reason: {ctx.Bot.phishingUrlSubmissionGuildBans[ctx.Guild.Id].Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, "Malicious Host Submissions"));
                return;
            }

            string host;

            try
            {
                host = new UriBuilder(url).Host;
            }
            catch (Exception)
            {
                embed.Description = $"`The host ('{url.SanitizeForCode()}') you're trying to submit is invalid.`";
                _ = RespondOrEdit(embed.AsError(ctx, "Malicious Host Submissions"));
                return;
            }

            embed.Description = $"`You are about to submit the host '{host.SanitizeForCode()}'. Do you want to proceed?`";
            embed.AsAwaitingInput(ctx, "Malicious Host Submissions");

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), "Submit host", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { MessageComponents.CancelButton }
            }));

            var e = await ctx.WaitForButtonAsync(TimeSpan.FromMinutes(2));

            if (e.TimedOut)
            {
                ModifyToTimedOut(true);
                return;
            }

            await e.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == ContinueButton.CustomId)
            {
                embed.Description = $"`Submitting your host..`";
                embed.AsLoading(ctx, "Malicious Host Submissions");
                await RespondOrEdit(embed);

                embed.Description = $"`Checking if your host is already in the database..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.phishingUrls)
                {
                    if (host.Contains(b.Key))
                    {
                        embed.Description = $"`The host ('{host.SanitizeForCode()}') is already present in the database. Thanks for trying to contribute regardless.`";
                        embed.AsError(ctx, "Malicious Host Submissions");
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Checking if your host has already been submitted before..`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.submittedUrls)
                {
                    if (b.Value.Url == host)
                    {
                        embed.Description = $"`The host ('{host.SanitizeForCode()}') has already been submitted. Thanks for trying to contribute regardless.`";
                        embed.AsError(ctx, "Malicious Host Submissions");
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`Creating submission..`";
                await RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.UrlSubmissions);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = StatusIndicatorIcons.Success, Name = $"Malicious Host Submissions" },
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Submitted host`: `{host.SanitizeForCode()}`\n" +
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
                    Url = host,
                    Submitter = ctx.User.Id,
                    GuildOrigin = ctx.Guild.Id
                });

                ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime = DateTime.UtcNow;

                embed.Description = $"`Submission created. Thanks for your contribution.`";
                embed.AsSuccess(ctx, "Malicious Host Submissions");
                await RespondOrEdit(embed);
            }
            else if (e.GetCustomId() == MessageComponents.CancelButton.CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}