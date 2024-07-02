// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.ScoreSaber;

namespace ProjectMakoto.Commands;

internal sealed class ScoreSaberMapLeaderboardCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var boardId = (int)arguments["leaderboardid"];
            var Page = (uint)arguments["page"];
            var Internal_Page = (int)arguments["internal_page"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            if (Page <= 0 || !(Internal_Page is 0 or 1))
            {
                this.SendSyntaxError();
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.ScoreSaber.MapLeaderboard.LoadingScoreboard, true)
            }.AsLoading(ctx, "Score Saber");

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

            var NextPageId = Guid.NewGuid().ToString();
            var PrevPageId = Guid.NewGuid().ToString();

            var InternalPage = Internal_Page;

            var scoreSaberPage = Page;

            Leaderboard leaderboard;

            int TotalPages;

            try
            {
                leaderboard = await ctx.Bot.ScoreSaberClient.GetScoreboardById(boardId.ToString());
                var scores = await ctx.Bot.ScoreSaberClient.GetScoreboardScoresById(boardId.ToString());

                TotalPages = scores.Metadata.TotalPages / scores.Metadata.ItemCount;
            }
            catch (InternalServerErrorException)
            {
                embed.Description = this.GetString(this.t.Commands.ScoreSaber.InternalServerError, true);
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                return;
            }
            catch (NotFoundException)
            {
                embed.Description = this.GetString(this.t.Commands.ScoreSaber.MapLeaderboard.ScoreboardNotExist, true);
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                throw;
            }
            catch (ForbiddenException)
            {
                embed.Description = this.GetString(this.t.Commands.ScoreSaber.ForbiddenError, true);
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                return;
            }
            catch (Exception)
            {
                throw;
            }

            CancellationTokenSource cancellationTokenSource = new();
            Dictionary<uint, LeaderboardScores> cachedPages = new();

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
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

                            this.ModifyToTimedOut();
                        }
                        catch { }
                    }
                }).Add(ctx.Bot, ctx);
            }

            async Task SendPage(int internalPage, uint scoreSaberPage)
            {
                if (scoreSaberPage > TotalPages)
                {
                    embed.Description = this.GetString(this.t.Commands.ScoreSaber.MapLeaderboard.PageNotExist, true, new TVar("Page", scoreSaberPage));
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    return;
                }

                LeaderboardScores scores;
                try
                {
                    if (!cachedPages.ContainsKey(scoreSaberPage))
                        cachedPages.Add(scoreSaberPage, await ctx.Bot.ScoreSaberClient.GetScoreboardScoresById(boardId.ToString(), scoreSaberPage));

                    scores = cachedPages[scoreSaberPage];
                }
                catch (InternalServerErrorException)
                {
                    embed.Description = this.GetString(this.t.Commands.ScoreSaber.InternalServerError, true);
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    return;
                }
                catch (NotFoundException)
                {
                    embed.Description = this.GetString(this.t.Commands.ScoreSaber.MapLeaderboard.ScoreboardNotExist, true);
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    throw;
                }
                catch (ForbiddenException)
                {
                    embed.Description = this.GetString(this.t.Commands.ScoreSaber.ForbiddenError, true);
                    _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
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
                embed.Footer = ctx.GenerateUsedByFooter($"{this.GetString(this.t.Common.Page)} {scoreSaberPage}/{TotalPages}");
                _ = embed.ClearFields();
                foreach (var score in scores.Scores.ToList().Skip(internalPage * 6).Take(6))
                {
                    _ = embed.AddField(new DiscordEmbedField($"**#{score.Rank}** {score.Player.Country.IsoCountryCodeToFlagEmoji()} `{score.Player.Name.SanitizeForCode()}`󠂪 󠂪| 󠂪 󠂪{Formatter.Timestamp(score.Timestamp, TimestampFormat.RelativeTime)}",
                        $"{(leaderboard.leaderboardInfo.ranked ? $"**`{((decimal)((decimal)score.ModifiedScore / (decimal)leaderboard.leaderboardInfo.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.PP).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`**󠂪 󠂪| 󠂪 󠂪" : "󠂪 󠂪| 󠂪 󠂪")}" +
                        $"`{score.ModifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}`󠂪 󠂪| 󠂪 󠂪**{(score.FullCombo ? "✅ `FC`" : $"{false.ToEmote(ctx.Bot)} `{score.MissedNotes + score.BadCuts}`")}**\n" +
                        $"{this.GetString(this.t.Commands.ScoreSaber.MapLeaderboard.Profile)}: `{ctx.Prefix}scoresaber profile {score.Player.Id}`"));
                }

                var previousPageButton = new DiscordButtonComponent(ButtonStyle.Primary, PrevPageId, this.GetString(this.t.Common.PreviousPage), (scoreSaberPage + InternalPage - 1 <= 0), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
                var nextPageButton = new DiscordButtonComponent(ButtonStyle.Primary, NextPageId, this.GetString(this.t.Common.NextPage), (scoreSaberPage + 1 > scores.Metadata.TotalPages / scores.Metadata.ItemCount), new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(new List<DiscordComponent> { previousPageButton, nextPageButton }));
            };

            await SendPage(InternalPage, scoreSaberPage);

            try
            {
                await Task.Delay(60000, cancellationTokenSource.Token);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                this.ModifyToTimedOut();
            }
            catch { }
        });
    }
}