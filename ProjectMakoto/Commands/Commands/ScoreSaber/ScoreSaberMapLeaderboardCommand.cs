// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class ScoreSaberMapLeaderboardCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            int boardId = (int)arguments["boardId"];
            int Page = (int)arguments["Page"];
            int Internal_Page = (int)arguments["Internal_Page"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            if (Page <= 0 || !(Internal_Page is 0 or 1))
            {
                SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = GetString(this.t.Commands.ScoreSaber.MapLeaderboard.LoadingScoreboard, true)
            }.AsLoading(ctx, "Score Saber");

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
                embed.Description = GetString(this.t.Commands.ScoreSaber.InternalServerError, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                return;
            }
            catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
            {
                embed.Description = GetString(this.t.Commands.ScoreSaber.MapLeaderboard.ScoreboardNotExist, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                throw;
            }
            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
            {
                embed.Description = GetString(this.t.Commands.ScoreSaber.ForbiddenError, true);
                await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
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

                        if (e.GetCustomId() == NextPageId)
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
                        else if (e.GetCustomId() == PrevPageId)
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
                }).Add(ctx.Bot, ctx);
            }

            async Task SendPage(int internalPage, int scoreSaberPage)
            {
                if (scoreSaberPage > TotalPages)
                {
                    embed.Description = GetString(this.t.Commands.ScoreSaber.MapLeaderboard.PageNotExist, true, new TVar("Page", scoreSaberPage));
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
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
                    embed.Description = GetString(this.t.Commands.ScoreSaber.InternalServerError, true);
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    return;
                }
                catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
                {
                    embed.Description = GetString(this.t.Commands.ScoreSaber.MapLeaderboard.ScoreboardNotExist, true);
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    throw;
                }
                catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                {
                    embed.Description = GetString(this.t.Commands.ScoreSaber.ForbiddenError, true);
                    await RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    return;
                }
                catch (Exception)
                {
                    throw;
                }

                embed = embed.AsInfo(ctx, "Score Saber");
                embed.Title = $"{leaderboard.leaderboardInfo.songName.FullSanitize()}{(!string.IsNullOrWhiteSpace(leaderboard.leaderboardInfo.songSubName) ? $" {leaderboard.leaderboardInfo.songSubName.FullSanitize()}" : "")} - {leaderboard.leaderboardInfo.songAuthorName.FullSanitize()} [{leaderboard.leaderboardInfo.levelAuthorName.FullSanitize()}]".TruncateWithIndication(256);
                embed.Description = "";
                embed.Author.IconUrl = ctx.Guild.IconUrl;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = leaderboard.leaderboardInfo.coverImage };
                embed.Footer = ctx.GenerateUsedByFooter($"{GetString(this.t.Common.Page)} {scoreSaberPage}/{TotalPages}");
                embed.ClearFields();
                foreach (var score in scores.scores.ToList().Skip(internalPage * 6).Take(6))
                {
                    embed.AddField(new DiscordEmbedField($"**#{score.rank}** {score.leaderboardPlayerInfo.country.IsoCountryCodeToFlagEmoji()} `{score.leaderboardPlayerInfo.name.SanitizeForCode()}`󠂪 󠂪| 󠂪 󠂪{Formatter.Timestamp(score.timeSet, TimestampFormat.RelativeTime)}",
                        $"{(leaderboard.leaderboardInfo.ranked ? $"**`{((decimal)((decimal)score.modifiedScore / (decimal)leaderboard.leaderboardInfo.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`**󠂪 󠂪| 󠂪 󠂪" : "󠂪 󠂪| 󠂪 󠂪")}" +
                        $"`{score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}`󠂪 󠂪| 󠂪 󠂪**{(score.fullCombo ? "✅ `FC`" : $"{false.ToEmote(ctx.Bot)} `{score.missedNotes + score.badCuts}`")}**\n" +
                        $"{GetString(this.t.Commands.ScoreSaber.MapLeaderboard.Profile)}: `{ctx.Prefix}scoresaber profile {score.leaderboardPlayerInfo.id}`"));
                }

                var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, GetString(this.t.Common.PreviousPage), (scoreSaberPage + InternalPage - 1 <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, GetString(this.t.Common.NextPage), (scoreSaberPage + 1 > scores.metadata.total / scores.metadata.itemsPerPage), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

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