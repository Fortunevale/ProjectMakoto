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
                var button = new DiscordButtonComponent(ButtonStyle.Primary, "accepted-tos", GetString(t.Commands.ReportHost.AcceptTos), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍")));

                var tos_embed = new DiscordEmbedBuilder
                {
                    Description = GetString(t.Commands.ReportHost.Tos)
                        .Replace("{1}", 1.ToEmotes())
                        .Replace("{2}", 2.ToEmotes())
                        .Replace("{3}", 3.ToEmotes())
                        .Replace("{4}", 4.ToEmotes())
                        .Build()
                }.AsAwaitingInput(ctx, GetString(t.Commands.ReportHost.Title));

                if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS != 0 && ctx.Bot.users[ctx.User.Id].UrlSubmissions.AcceptedTOS < tos_version)
                {
                    tos_embed.Description = tos_embed.Description.Insert(0, $"**{GetString(t.Commands.ReportHost.TosChangedNotice)}**\n\n");
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
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`{GetString(t.Commands.ReportHost.Processing)}`"
            }.AsLoading(ctx, GetString(t.Commands.ReportHost.Title));

            await RespondOrEdit(embed);

            if (ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45) > DateTime.UtcNow && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                embed.Description = $"`{GetString(t.Commands.ReportHost.CooldownError).Replace("{Timestamp}", $"`{ctx.Bot.users[ctx.User.Id].UrlSubmissions.LastTime.AddMinutes(45).ToTimestamp()}`")}`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.ReportHost.Title)));
                return;
            }

            if (ctx.Bot.submittedUrls.Any(x => x.Value.Submitter == ctx.User.Id) && !ctx.User.IsMaintenance(ctx.Bot.status))
            {
                if (ctx.Bot.submittedUrls.Where(x => x.Value.Submitter == ctx.User.Id).Count() >= 5)
                {
                    embed.Description = $"`{GetString(t.Commands.ReportHost.LimitError)}`";
                    _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.ReportHost.Title)));
                    return;
                }
            }

            if (ctx.Bot.phishingUrlSubmissionUserBans.ContainsKey(ctx.User.Id))
            {
                embed.Description = $"`{GetString(t.Commands.ReportHost.UserBan)}`\n" +
                                    $"`{GetString(t.Common.Reason)}: {ctx.Bot.phishingUrlSubmissionUserBans[ctx.User.Id].Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.ReportHost.Title)));
                return;
            }

            if (ctx.Bot.phishingUrlSubmissionGuildBans.ContainsKey(ctx.Guild.Id))
            {
                embed.Description = $"`{GetString(t.Commands.ReportHost.GuildBan)}`\n" +
                                    $"`{GetString(t.Common.Reason)}: {ctx.Bot.phishingUrlSubmissionGuildBans[ctx.Guild.Id].Reason}`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.ReportHost.Title)));
                return;
            }

            string host;

            try
            {
                host = new UriBuilder(url).Host;
            }
            catch (Exception)
            {
                embed.Description = $"`{GetString(t.Commands.ReportHost.InvalidHost).Replace("{Host}", url.SanitizeForCode())}.`";
                _ = RespondOrEdit(embed.AsError(ctx, GetString(t.Commands.ReportHost.Title)));
                return;
            }

            embed.Description = $"`{GetString(t.Commands.ReportHost.ConfirmHost).Replace("{Host}", host.SanitizeForCode())}`";
            embed.AsAwaitingInput(ctx, GetString(t.Commands.ReportHost.Title));

            var ContinueButton = new DiscordButtonComponent(ButtonStyle.Success, Guid.NewGuid().ToString(), GetString(t.Common.Confirm), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent>
            {
                { ContinueButton },
                { MessageComponents.GetCancelButton(ctx.DbUser) }
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
                embed.AsLoading(ctx, GetString(t.Commands.ReportHost.Title));

                embed.Description = $"`{GetString(t.Commands.ReportHost.DatabaseCheck)}`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.phishingUrls)
                {
                    if (host.Contains(b.Key))
                    {
                        embed.Description = $"`{GetString(t.Commands.ReportHost.DatabaseError).Replace("{Host}", host.SanitizeForCode())}`";
                        embed.AsError(ctx, GetString(t.Commands.ReportHost.Title));
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`{GetString(t.Commands.ReportHost.SubmissionCheck)}`";
                await RespondOrEdit(embed);

                foreach (var b in ctx.Bot.submittedUrls)
                {
                    if (b.Value.Url == host)
                    {
                        embed.Description = $"`{GetString(t.Commands.ReportHost.SubmissionError).Replace("{Host}", host.SanitizeForCode())}`";
                        embed.AsError(ctx, GetString(t.Commands.ReportHost.Title));
                        _ = RespondOrEdit(embed.Build());
                        return;
                    }
                }

                embed.Description = $"`{GetString(t.Commands.ReportHost.CreatingSubmission)}`";
                await RespondOrEdit(embed);

                var channel = await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.UrlSubmissions);

                var AcceptSubmission = new DiscordButtonComponent(ButtonStyle.Success, "accept_submission", "Accept submission", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")));
                var DenySubmission = new DiscordButtonComponent(ButtonStyle.Danger, "deny_submission", "Deny submission", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanUserButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_user", "Deny submission & ban submitter", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));
                var BanGuildButton = new DiscordButtonComponent(ButtonStyle.Danger, "ban_guild", "Deny submission & ban guild", false, new DiscordComponentEmoji(DiscordEmoji.FromGuildEmote(ctx.Client, 1005430134070841395)));

                var subbmited_msg = await channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = StatusIndicatorIcons.Success, Name = GetString(t.Commands.ReportHost.Title) },
                    Color = EmbedColors.Success,
                    Timestamp = DateTime.UtcNow,
                    Description = $"`Submitted host`: `{host.SanitizeForCode()}`\n" +
                                  $"`Submission by `: `{ctx.User.UsernameWithDiscriminator} ({ctx.User.Id})`\n" +
                                  $"`Submitted on  `: `{ctx.Guild.Name} ({ctx.Guild.Id})`"
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

                embed.Description = $"`{GetString(t.Commands.ReportHost.SubmissionCreated)}`";
                embed.AsSuccess(ctx, GetString(t.Commands.ReportHost.Title));
                await RespondOrEdit(embed);
            }
            else if (e.GetCustomId() == MessageComponents.GetCancelButton(ctx.DbUser).CustomId)
            {
                DeleteOrInvalidate();
            }
        });
    }
}