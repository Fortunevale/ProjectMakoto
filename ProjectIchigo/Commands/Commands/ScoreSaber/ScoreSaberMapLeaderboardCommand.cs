namespace ProjectIchigo.Commands;

internal class ScoreSaberMapLeaderboardCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int boardId = (int)arguments["boardId"];
            int Page = (int)arguments["Page"];
            int Internal_Page = (int)arguments["Internal_Page"];

            if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
                return;

            if (Page <= 0 || !(Internal_Page is 0 or 1))
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"`Looking for scoreboard..`"
            }.SetLoading(ctx, "Score Saber");

            await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

            string NextPageId = Guid.NewGuid().ToString();
            string PrevPageId = Guid.NewGuid().ToString();

            int InternalPage = Internal_Page;

            int scoreSaberPage = Page;

            Leaderboard leaderboard;

            int TotalPages;

            try
            {
                leaderboard = await ctx.Bot.scoreSaberClient.GetScoreboardById(boardId.ToString());
                LeaderboardScores scores = await ctx.Bot.scoreSaberClient.GetScoreboardScoresById(boardId.ToString());

                TotalPages = scores.metadata.total / scores.metadata.itemsPerPage;
            }
            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
            {
                embed.Description = $"`An internal server exception occurred. Please retry later.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                return;
            }
            catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
            {
                embed.Description = $"`The requested scoreboard does not exist.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                throw;
            }
            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
            {
                embed.Description = $"`The access to the search api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                return;
            }
            catch (Exception)
            {
                throw;
            }

            CancellationTokenSource cancellationTokenSource = new();
            Dictionary<int, LeaderboardScores> cachedPages = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        if (e.Interaction.Data.CustomId == NextPageId)
                        {
                            if (InternalPage == 1)
                            {
                                InternalPage = 0;

                                scoreSaberPage++;
                            }
                            else if (InternalPage == 0)
                            {
                                InternalPage = 1;
                            }

                            await SendPage(InternalPage, scoreSaberPage);
                        }
                        else if (e.Interaction.Data.CustomId == PrevPageId)
                        {
                            if (InternalPage == 1)
                            {
                                InternalPage = 0;
                            }
                            else if (InternalPage == 0)
                            {
                                InternalPage = 1;

                                scoreSaberPage--;
                            }

                            await SendPage(InternalPage, scoreSaberPage);
                        }

                        try
                        {
                            await Task.Delay(60000, cancellationTokenSource.Token);

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            ModifyToTimedOut();
                        }
                        catch { }
                    }
                }).Add(ctx.Bot.watcher, ctx);
            }

            async Task SendPage(int internalPage, int scoreSaberPage)
            {
                if (scoreSaberPage > TotalPages)
                {
                    embed.Description = $"`Page {scoreSaberPage} doesn't exist.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                    return;
                }

                LeaderboardScores scores;
                try
                {
                    if (!cachedPages.ContainsKey(scoreSaberPage))
                        cachedPages.Add(scoreSaberPage, await ctx.Bot.scoreSaberClient.GetScoreboardScoresById(boardId.ToString(), scoreSaberPage));

                    scores = cachedPages[scoreSaberPage];
                }
                catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                {
                    embed.Description = $"`An internal server exception occurred. Please retry later.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                    return;
                }
                catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
                {
                    embed.Description = $"`The requested scoreboard does not exist.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                    throw;
                }
                catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                {
                    embed.Description = $"`The access to the search api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.SetError(ctx, "Score Saber")));
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                embed = embed.SetInfo(ctx, "Score Saber");
                embed.Title = $"{leaderboard.leaderboardInfo.songName.Sanitize()}{(!string.IsNullOrWhiteSpace(leaderboard.leaderboardInfo.songSubName) ? $" {leaderboard.leaderboardInfo.songSubName.Sanitize()}" : "")} - {leaderboard.leaderboardInfo.songAuthorName.Sanitize()} [{leaderboard.leaderboardInfo.levelAuthorName.Sanitize()}]".TruncateWithIndication(256);
                embed.Description = "";
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = leaderboard.leaderboardInfo.coverImage };
                embed.Footer = ctx.GenerateUsedByFooter($"Page {scoreSaberPage}/{TotalPages}");
                embed.ClearFields();
                foreach (var score in scores.scores.ToList().Skip(internalPage * 6).Take(6))
                {
                    embed.AddField(new DiscordEmbedField($"**#{score.rank}** {score.leaderboardPlayerInfo.country.IsoCountryCodeToFlagEmoji()} `{score.leaderboardPlayerInfo.name.SanitizeForCodeBlock()}`󠂪 󠂪| 󠂪 󠂪{Formatter.Timestamp(score.timeSet, TimestampFormat.RelativeTime)}",
                        $"{(leaderboard.leaderboardInfo.ranked ? $"**`{((decimal)((decimal)score.modifiedScore / (decimal)leaderboard.leaderboardInfo.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`**󠂪 󠂪| 󠂪 󠂪" : "󠂪 󠂪| 󠂪 󠂪")}" +
                        $"`{score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}`󠂪 󠂪| 󠂪 󠂪**{(score.fullCombo ? "✅ `FC`" : $"{false.BoolToEmote(ctx.Client)} `{score.missedNotes + score.badCuts}`")}**\n" +
                        $"Profile: `{ctx.Prefix}scoresaber profile {score.leaderboardPlayerInfo.id}`"));
                }

                var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, "Previous page", (scoreSaberPage + InternalPage - 1 <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, "Next page", (scoreSaberPage + 1 > scores.metadata.total / scores.metadata.itemsPerPage), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { previousPageButton, nextPageButton }));
            };

            await SendPage(InternalPage, scoreSaberPage);

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                ModifyToTimedOut();
            }
            catch { }
        });
    }
}