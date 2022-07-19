﻿namespace ProjectIchigo.Commands;

internal class ScoreSaberCommandAbstractions
{
    internal static async Task SendScoreSaberProfile(SharedCommandContext ctx, string id = "", bool AddLinkButton = true)
    {
        if (!ctx.Bot._users.List.ContainsKey(ctx.User.Id))
            ctx.Bot._users.List.Add(ctx.User.Id, new Users.Info(ctx.Bot));

        if (string.IsNullOrWhiteSpace(id))
        {
            if (ctx.Bot._users.List[ctx.User.Id].ScoreSaber.Id != 0)
            {
                id = ctx.Bot._users.List[ctx.User.Id].ScoreSaber.Id.ToString();
            }
            else
            {
                ctx.BaseCommand.SendSyntaxError();
                return;
            }
        }

        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Score Saber • {ctx.Guild.Name}" },
            Color = EmbedColors.Processing,
            Footer = ctx.GenerateUsedByFooter(),
            Timestamp = DateTime.UtcNow,
            Description = $"`Looking for player..`"
        };

        await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

        try
        {
            var player = await ctx.Bot._scoreSaberClient.GetPlayerById(id);

            CancellationTokenSource cancellationTokenSource = new();

            DiscordButtonComponent ShowProfileButton = new(ButtonStyle.Primary, "getmain", "Show Profile", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            DiscordButtonComponent TopScoresButton = new(ButtonStyle.Primary, "gettopscores", "Show Top Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎇")));
            DiscordButtonComponent RecentScoresButton = new(ButtonStyle.Primary, "getrecentscores", "Show Recent Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

            DiscordButtonComponent LinkButton = new(ButtonStyle.Primary, "thats_me", "Link Score Saber Profile to Discord Account", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));

            DiscordLinkButtonComponent OpenProfileInBrowser = new($"https://scoresaber.com/u/{id}", "Open in browser", false);

            List<DiscordComponent> ProfileInteractionRow = new();
            ProfileInteractionRow.Add(OpenProfileInBrowser);
            ProfileInteractionRow.Add(TopScoresButton);
            ProfileInteractionRow.Add(RecentScoresButton);

            List<DiscordComponent> RecentScoreInteractionRow = new();
            RecentScoreInteractionRow.Add(OpenProfileInBrowser);
            RecentScoreInteractionRow.Add(ShowProfileButton);
            RecentScoreInteractionRow.Add(TopScoresButton);

            List<DiscordComponent> TopScoreInteractionRow = new();
            TopScoreInteractionRow.Add(OpenProfileInBrowser);
            TopScoreInteractionRow.Add(ShowProfileButton);
            TopScoreInteractionRow.Add(RecentScoresButton);

            PlayerScores CachedTopScores = null;
            PlayerScores CachedRecentScores = null;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == "thats_me")
                        {
                            AddLinkButton = false;
                            ShowProfile().Add(ctx.Bot._watcher, ctx);
                            ctx.Bot._users.List[ctx.User.Id].ScoreSaber.Id = Convert.ToUInt64(player.id);

                            var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber • {ctx.Guild.Name}" },
                                Color = EmbedColors.Success,
                                Footer = ctx.GenerateUsedByFooter("This message automatically deletes in 10 seconds"),
                                Timestamp = DateTime.UtcNow,
                                Description = $"{ctx.User.Mention} `Linked '{player.name.SanitizeForCodeBlock()}' ({player.id}) to your account. You can now run '{ctx.Prefix}scoresaber' without an argument to get your profile in an instant.`\n" +
                                              $"`To remove the link, run '{ctx.Prefix}scoresaber-unlink'.`"
                            }));

                            _ = Task.Delay(10000).ContinueWith(x =>
                            {
                                _ = new_msg.DeleteAsync();
                            });
                        }
                        else if (e.Interaction.Data.CustomId == "gettopscores")
                        {
                            try
                            {
                                if (CachedTopScores is null)
                                    CachedTopScores = await ctx.Bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.TOP);

                                ShowScores(CachedTopScores, RequestParameters.ScoreType.TOP).Add(ctx.Bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = EmbedColors.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = EmbedColors.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.Interaction.Data.CustomId == "getrecentscores")
                        {
                            try
                            {
                                if (CachedRecentScores is null)
                                    CachedRecentScores = await ctx.Bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.RECENT);

                                ShowScores(CachedRecentScores, RequestParameters.ScoreType.RECENT).Add(ctx.Bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = EmbedColors.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = EmbedColors.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.Interaction.Data.CustomId == "getmain")
                        {
                            ShowProfile().Add(ctx.Bot._watcher, ctx);
                        }

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(OpenProfileInBrowser));

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                    }
                }).Add(ctx.Bot._watcher, ctx);
            }

            async Task ShowScores(PlayerScores scores, RequestParameters.ScoreType scoreType)
            {
                embed.ClearFields();
                embed.ImageUrl = "";
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n\n" +
                                    $"{(scoreType == RequestParameters.ScoreType.TOP ? "**Top Scores**" : "**Recent Scores**")}";

                foreach (var score in scores.playerScores.Take(5))
                {
                    decimal page = Math.Ceiling((decimal)score.score.rank / (decimal)12);

                    decimal rank = score.score.rank / 6;
                    bool odd = (rank % 2 != 0);

                    embed.AddField(new DiscordEmbedField($"{score.leaderboard.songName.Sanitize()}{(!string.IsNullOrWhiteSpace(score.leaderboard.songSubName) ? $" {score.leaderboard.songSubName.Sanitize()}" : "")} - {score.leaderboard.songAuthorName.Sanitize()} [{score.leaderboard.levelAuthorName.Sanitize()}]".TruncateWithIndication(256),
                        $":globe_with_meridians: **#{score.score.rank}**  󠂪 󠂪| 󠂪 󠂪 {Formatter.Timestamp(score.score.timeSet, TimestampFormat.RelativeTime)}\n" +
                        $"{(score.leaderboard.ranked ? $"**`{((decimal)((decimal)score.score.modifiedScore / (decimal)score.leaderboard.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp [{(score.score.pp * score.score.weight).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp]`**\n" : "\n")}" +
                        $"`{score.score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}` 󠂪 󠂪| 󠂪 󠂪 **{(score.score.fullCombo ? "✅ `FC`" : $"{false.BoolToEmote(ctx.Client)} `{score.score.missedNotes + score.score.badCuts}`")}**\n" +
                        $"Map Leaderboard: `{ctx.Prefix}scoresaber map-leaderboard {score.leaderboard.difficulty.leaderboardId} {page}{(odd ? " 1" : "")}`"));
                }

                DiscordMessageBuilder builder = new();

                if (ctx.Bot._users.List[ctx.User.Id].ScoreSaber.Id == 0 && AddLinkButton)
                    builder.AddComponents(LinkButton);

                await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed).AddComponents((scoreType == RequestParameters.ScoreType.TOP ? TopScoreInteractionRow : RecentScoreInteractionRow)));
            }

            string LoadedGraph = "";

            async Task ShowProfile()
            {
                embed.ClearFields();
                embed.Title = $"{player.name.Sanitize()} 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪`{player.pp.ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`";
                embed.Color = EmbedColors.Info;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.profilePicture };
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n";
                embed.AddField(new DiscordEmbedField("Ranked Play Count", $"`{player.scoreStats.rankedPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Ranked Score", $"`{player.scoreStats.totalRankedScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Average Ranked Accuracy", $"`{Math.Round(player.scoreStats.averageRankedAccuracy, 2).ToString().Replace(",", ".")}%`", true));
                embed.AddField(new DiscordEmbedField("Total Play Count", $"`{player.scoreStats.totalPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Score", $"`{player.scoreStats.totalScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Replays Watched By Others", $"`{player.scoreStats.replaysWatched}`", true));

                DiscordMessageBuilder builder = new();

                if (ctx.Bot._users.List[ctx.User.Id].ScoreSaber.Id == 0 && AddLinkButton)
                    builder.AddComponents(LinkButton);

                if (!string.IsNullOrWhiteSpace(LoadedGraph))
                {
                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.ImageUrl = LoadedGraph;
                    builder.AddComponents(ProfileInteractionRow);
                }

                await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed));

                var file = $"{Guid.NewGuid()}.png";

                if (string.IsNullOrWhiteSpace(LoadedGraph))
                    try
                    {
                        Chart qc = new();
                        qc.Width = 1000;
                        qc.Height = 500;
                        qc.Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    '',
                                    '48 days ago','',
                                    '46 days ago','',
                                    '44 days ago','',
                                    '42 days ago','',
                                    '40 days ago','',
                                    '38 days ago','',
                                    '36 days ago','',
                                    '34 days ago','',
                                    '32 days ago','',
                                    '30 days ago','',
                                    '28 days ago','',
                                    '26 days ago','',
                                    '24 days ago','',
                                    '22 days ago','',
                                    '20 days ago','',
                                    '18 days ago','',
                                    '16 days ago','',
                                    '14 days ago','',
                                    '12 days ago','',
                                    '10 days ago','',
                                    '8 days ago','',
                                    '6 days ago','',
                                    '4 days ago','',
                                    '2 days ago','',
                                    'Today'
                                ],
                                datasets: 
                                [
                                    {{
                                        label: 'Placements',
                                        data: [{player.histories},{player.rank}],
                                        fill: false,
                                        borderColor: getGradientFillHelper('vertical', ['#6b76da', '#a336eb', '#FC0000']),
                                        reverse: true,
                                        id: ""yaxis2""

                                    }}
                                ]

                            }},
                            options:
                            {{
                                legend:
                                {{
                                    display: false,
                                }},
                                elements:
                                {{
                                    point:
                                    {{
                                        radius: 0
                                    }}
                                }},
                                scales:
                                {{
                                    yAxes:
                                    [
                                        {{
                                            reverse: true,
                                            ticks:
                                            {{
                                                reverse: true
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        }}";

                        qc.ToFile(file);

                        using (FileStream stream = File.Open(file, FileMode.Open))
                        {
                            var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot._status.LoadedConfig.GraphAssetsChannelId)).SendMessageAsync(new DiscordMessageBuilder().WithFile(file, stream));

                            LoadedGraph = asset.Attachments[0].Url;

                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.ImageUrl = asset.Attachments[0].Url;
                            builder = builder.WithEmbed(embed);
                            builder.AddComponents(ProfileInteractionRow);
                            await ctx.BaseCommand.RespondOrEdit(builder);
                        }
                    }
                    catch (Exception ex)
                    {
                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        builder.AddComponents(ProfileInteractionRow);
                        await ctx.BaseCommand.RespondOrEdit(builder);
                        _logger.LogError(ex.ToString());
                    }

                try
                {
                    await Task.Delay(1000);
                    File.Delete(file);
                }
                catch { }
            }

            ShowProfile().Add(ctx.Bot._watcher, ctx);

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            try
            {
                await Task.Delay(120000, cancellationTokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(OpenProfileInBrowser));

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        }
        catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = EmbedColors.Error;
            embed.Description = $"`An internal server exception occured. Please retry later.`";
            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = EmbedColors.Error;
            embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = EmbedColors.Error;
            embed.Description = $"`Couldn't find the specified player.`";
            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.UnprocessableEntity)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = EmbedColors.Error;
            embed.Description = $"`Please provide an user id.`";
            await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
