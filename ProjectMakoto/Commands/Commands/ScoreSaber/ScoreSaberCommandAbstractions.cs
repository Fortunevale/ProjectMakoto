// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.ScoreSaber;
using ProjectMakoto.Enums.ScoreSaber;

namespace ProjectMakoto.Commands;

internal static class ScoreSaberCommandAbstractions
{
    internal static async Task SendScoreSaberProfile(SharedCommandContext ctx, string id = "", bool AddLinkButton = true)
    {
        var t = ctx.t;
        string GetString(SingleTranslationKey v, bool Code = false, params TVar[] vars)
            => ctx.BaseCommand.GetString(v, Code, vars);
        string GetMString(MultiTranslationKey v)
            => ctx.BaseCommand.GetString(v);

        if (string.IsNullOrWhiteSpace(id))
        {
            if (ctx.DbUser.ScoreSaber.Id != 0)
            {
                id = ctx.DbUser.ScoreSaber.Id.ToString();
            }
            else
            {
                ctx.BaseCommand.SendSyntaxError();
                return;
            }
        }

        var embed = new DiscordEmbedBuilder
        {
            Description = GetString(t.Commands.ScoreSaber.Profile.LoadingPlayer, true)
        }.AsLoading(ctx, "Score Saber");

        _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

        try
        {
            var player = await ctx.Bot.ScoreSaberClient.GetPlayerById(id);

            CancellationTokenSource cancellationTokenSource = new();

            DiscordButtonComponent ShowProfileButton = new(ButtonStyle.Primary, "getmain", GetString(t.Commands.ScoreSaber.Profile.ShowProfile), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👤")));
            DiscordButtonComponent TopScoresButton = new(ButtonStyle.Primary, "gettopscores", GetString(t.Commands.ScoreSaber.Profile.ShowTopScores), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🎇")));
            DiscordButtonComponent RecentScoresButton = new(ButtonStyle.Primary, "getrecentscores", GetString(t.Commands.ScoreSaber.Profile.ShowRecentScores), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🕒")));

            DiscordButtonComponent LinkButton = new(ButtonStyle.Primary, "thats_me", GetString(t.Commands.ScoreSaber.Profile.LinkProfileToAccount), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("↘")));

            DiscordLinkButtonComponent OpenProfileInBrowser = new($"https://scoresaber.com/u/{id}", GetString(t.Commands.ScoreSaber.Profile.OpenInBrowser), false);

            List<DiscordComponent> ProfileInteractionRow = new()
            {
                OpenProfileInBrowser,
                TopScoresButton,
                RecentScoresButton
            };

            List<DiscordComponent> RecentScoreInteractionRow = new()
            {
                OpenProfileInBrowser,
                ShowProfileButton,
                TopScoresButton
            };

            List<DiscordComponent> TopScoreInteractionRow = new()
            {
                OpenProfileInBrowser,
                ShowProfileButton,
                RecentScoresButton
            };

            PlayerScores CachedTopScores = null;
            PlayerScores CachedRecentScores = null;

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.GetCustomId() == "thats_me")
                        {
                            AddLinkButton = false;
                            _ = ShowProfile().Add(ctx.Bot, ctx);
                            ctx.DbUser.ScoreSaber.Id = Convert.ToUInt64(player.id);

                            var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Description = GetMString(t.Commands.ScoreSaber.Profile.LinkSuccessful).Build(
                                    new TVar("ProfileName", player.name),
                                    new TVar("ProfileId", player.id),
                                    new TVar("ProfileCommand", $"{ctx.Prefix}scoresaber profile"),
                                    new TVar("UnlinkCommand", $"{ctx.Prefix}scoresaber unlink"))
                            }.AsSuccess(ctx, "Score Saber")));

                            _ = Task.Delay(10000).ContinueWith(x =>
                            {
                                _ = new_msg.DeleteAsync();
                            });
                        }
                        else if (e.GetCustomId() == "gettopscores")
                        {
                            try
                            {
                                CachedTopScores ??= await ctx.Bot.ScoreSaberClient.GetScoresById(id, ScoreType.Top);

                                _ = ShowScores(CachedTopScores, ScoreType.Top).Add(ctx.Bot, ctx);
                            }
                            catch (InternalServerErrorException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = GetString(t.Commands.ScoreSaber.InternalServerError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));

                                return;
                            }
                            catch (ForbiddenException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = GetString(t.Commands.ScoreSaber.ForbiddenError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.GetCustomId() == "getrecentscores")
                        {
                            try
                            {
                                CachedRecentScores ??= await ctx.Bot.ScoreSaberClient.GetScoresById(id, ScoreType.Recent);

                                _ = ShowScores(CachedRecentScores, ScoreType.Recent).Add(ctx.Bot, ctx);
                            }
                            catch (InternalServerErrorException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = GetString(t.Commands.ScoreSaber.InternalServerError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (ForbiddenException)
                            {
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                                embed = embed.AsError(ctx, "Score Saber");
                                embed.Description = GetString(t.Commands.ScoreSaber.ForbiddenError, true);
                                _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed));
                                return;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (e.GetCustomId() == "getmain")
                        {
                            _ = ShowProfile().Add(ctx.Bot, ctx);
                        }

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;

                            ctx.BaseCommand.ModifyToTimedOut(true);
                        }
                        catch { }
                    }
                }).Add(ctx.Bot, ctx);
            }

            async Task ShowScores(PlayerScores scores, ScoreType scoreType)
            {
                _ = embed.ClearFields();
                embed.ImageUrl = "";
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n\n" +
                                    $"{(scoreType == ScoreType.Top ? $"**{GetString(t.Commands.ScoreSaber.Profile.TopScores)}**" : $"**{GetString(t.Commands.ScoreSaber.Profile.TopScores)}**")}";

                foreach (var score in scores.playerScores.Take(5))
                {
                    var page = Math.Ceiling((decimal)score.score.rank / (decimal)12);

                    decimal rank = score.score.rank / 6;
                    var odd = (rank % 2 != 0);

                    _ = embed.AddField(new DiscordEmbedField($"{score.leaderboard.songName.FullSanitize()}{(!string.IsNullOrWhiteSpace(score.leaderboard.songSubName) ? $" {score.leaderboard.songSubName.FullSanitize()}" : "")} - {score.leaderboard.songAuthorName.FullSanitize()} [{score.leaderboard.levelAuthorName.FullSanitize()}]".TruncateWithIndication(256),
                        $":globe_with_meridians: **#{score.score.rank}**  󠂪 󠂪| 󠂪 󠂪 {Formatter.Timestamp(score.score.timeSet, TimestampFormat.RelativeTime)}\n" +
                        $"{(score.leaderboard.ranked ? $"**`{((decimal)((decimal)score.score.modifiedScore / (decimal)score.leaderboard.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp [{(score.score.pp * score.score.weight).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp]`**\n" : "\n")}" +
                        $"`{score.score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}` 󠂪 󠂪| 󠂪 󠂪 **{(score.score.fullCombo ? "✅ `FC`" : $"{false.ToEmote(ctx.Bot)} `{score.score.missedNotes + score.score.badCuts}`")}**\n" +
                        $"{GetString(t.Commands.ScoreSaber.Profile.MapLeaderboard)}: `{ctx.Prefix}scoresaber map-leaderboard {score.leaderboard.difficulty.leaderboardId} {page}{(odd ? " 1" : "")}`"));
                }

                DiscordMessageBuilder builder = new();

                if (ctx.DbUser.ScoreSaber.Id == 0 && AddLinkButton)
                    _ = builder.AddComponents(LinkButton);

                _ = await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed).AddComponents((scoreType == ScoreType.Top ? TopScoreInteractionRow : RecentScoreInteractionRow)));
            }

            var LoadedGraph = "";

            async Task ShowProfile()
            {
                embed = embed.AsInfo(ctx, "Score Saber");

                _ = embed.ClearFields();
                embed.Title = $"{player.name.FullSanitize()} 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪`{player.pp.ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp`";
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.profilePicture };
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n";
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.RankedPlayCount), $"`{player.scoreStats.rankedPlayCount}`", true));
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.TotalRankedScore), $"`{player.scoreStats.totalRankedScore.ToString("N0", CultureInfo.GetCultureInfo("en-US"))}`", true));
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.AverageRankedAccuracy), $"`{Math.Round(player.scoreStats.averageRankedAccuracy, 2).ToString().Replace(",", ".")}%`", true));
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.TotalPlayCount), $"`{player.scoreStats.totalPlayCount}`", true));
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.TotalScore), $"`{player.scoreStats.totalScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                _ = embed.AddField(new DiscordEmbedField(GetString(t.Commands.ScoreSaber.Profile.ReplaysWatched), $"`{player.scoreStats.replaysWatched}`", true));

                DiscordMessageBuilder builder = new();

                if (ctx.DbUser.ScoreSaber.Id == 0 && AddLinkButton)
                    _ = builder.AddComponents(LinkButton);

                if (!string.IsNullOrWhiteSpace(LoadedGraph))
                {
                    embed = embed.AsInfo(ctx, "Score Saber");
                    embed.ImageUrl = LoadedGraph;
                    _ = builder.AddComponents(ProfileInteractionRow);
                }

                _ = await ctx.BaseCommand.RespondOrEdit(builder.WithEmbed(embed));

                var file = $"{Guid.NewGuid()}.png";

                var labels = "";

                for (var i = 50; i >= 0; i -= 2)
                {
                    if (i == 0)
                    {
                        labels += $"'{t.Commands.ScoreSaber.Profile.GraphToday}'\n";
                        break;
                    }
                    if (i == 2)
                    {
                        labels += $"'{GetString(t.Commands.ScoreSaber.Profile.GraphDays).Build(new TVar("Count", i))}',\n";
                        continue;
                    }

                    labels += $"'{GetString(t.Commands.ScoreSaber.Profile.GraphDays).Build(new TVar("Count", i))}','',\n";
                }

                if (string.IsNullOrWhiteSpace(LoadedGraph))
                    try
                    {
                        Chart qc = new()
                        {
                            Width = 1000,
                            Height = 500,
                            Config = $@"{{
                            type: 'line',
                            data: 
                            {{
                                labels: 
                                [
                                    {labels}
                                ],
                                datasets: 
                                [
                                    {{
                                        label: '{GetString(t.Commands.ScoreSaber.Profile.Placement)}',
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
                        }}"
                        };

                        qc.ToFile(file);

                        using var stream = File.Open(file, FileMode.Open);

                        var asset = await (await ctx.Client.GetChannelAsync(ctx.Bot.status.LoadedConfig.Channels.GraphAssets)).SendMessageAsync(new DiscordMessageBuilder().WithFile(file, stream));

                        LoadedGraph = asset.Attachments[0].Url;

                        embed = embed.AsInfo(ctx, "Score Saber");
                        embed.ImageUrl = asset.Attachments[0].Url;
                        builder = builder.WithEmbed(embed);
                        _ = builder.AddComponents(ProfileInteractionRow);
                        _ = await ctx.BaseCommand.RespondOrEdit(builder);
                    }
                    catch (Exception ex)
                    {
                        embed = embed.AsInfo(ctx, "Score Saber");
                        _ = builder.AddComponents(ProfileInteractionRow);
                        _ = await ctx.BaseCommand.RespondOrEdit(builder);
                        _logger.LogError(ex.ToString());
                    }

                try
                {
                    await Task.Delay(1000);
                    File.Delete(file);
                }
                catch { }
            }

            _ = ShowProfile().Add(ctx.Bot, ctx);

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            try
            {
                await Task.Delay(120000, cancellationTokenSource.Token);

                ctx.Client.ComponentInteractionCreated -= RunInteraction;

                ctx.BaseCommand.ModifyToTimedOut(true);
            }
            catch { }
        }
        catch (InternalServerErrorException)
        {
            embed.Description = GetString(t.Commands.ScoreSaber.InternalServerError, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (ForbiddenException)
        {
            embed.Description = GetString(t.Commands.ScoreSaber.ForbiddenError, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (NotFoundException)
        {
            embed.Description = GetString(t.Commands.ScoreSaber.Profile.InvalidId, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (UnprocessableEntityException)
        {
            embed.Description = GetString(t.Commands.ScoreSaber.Profile.InvalidId, true);
            _ = await ctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
