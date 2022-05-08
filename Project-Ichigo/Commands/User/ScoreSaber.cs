namespace Project_Ichigo.Commands.User;

internal class ScoreSaber : BaseCommandModule
{
    public Bot _bot { private get; set; }



    private async Task SendScoreSaberProfile(CommandContext ctx, string id = "", bool AddLinkButton = true)
    {
        if (!_bot._users.List.ContainsKey(ctx.User.Id))
            _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

        if (string.IsNullOrWhiteSpace(id))
        {
            if (_bot._users.List[ctx.User.Id].ScoreSaber.Id != 0)
            {
                id = _bot._users.List[ctx.User.Id].ScoreSaber.Id.ToString();
            }
            else
            {
                _ = ctx.SendSyntaxError();
                return;
            }
        }

        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.StatusIndicators.DiscordCircleLoading, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
            Color = ColorHelper.Processing,
            Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
            Timestamp = DateTime.UtcNow,
            Description = $"`Looking for player..`"
        };

        var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));

        try
        {
            var player = await _bot._scoreSaberClient.GetPlayerById(id);

            CancellationTokenSource cancellationTokenSource = new();

            DiscordButtonComponent ShowProfileButton = new(ButtonStyle.Primary, "getmain", "Show Profile", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")));
            DiscordButtonComponent TopScoresButton = new(ButtonStyle.Primary, "gettopscores", "Show Top Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":sparkler:")));
            DiscordButtonComponent RecentScoresButton = new(ButtonStyle.Primary, "getrecentscores", "Show Recent Scores", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":clock3:")));

            DiscordButtonComponent LinkButton = new(ButtonStyle.Primary, "thats_me", "Link Score Saber Profile to Discord Account", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_lower_right:")));

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

            async Task RunInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                    {
                        _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        if (e.Interaction.Data.CustomId == "thats_me")
                        {
                            AddLinkButton = false;
                            ShowProfile().Add(_bot._watcher, ctx);
                            _bot._users.List[ctx.User.Id].ScoreSaber.Id = Convert.ToUInt64(player.id);

                            var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                            {
                                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                                Color = ColorHelper.Success,
                                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                                Timestamp = DateTime.UtcNow,
                                Description = $"{ctx.User.Mention} `Linked '{player.name}' ({player.id}) to your account. You can now run '{ctx.Prefix}scoresaber' without an argument to get your profile in an instant.`\n" +
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
                                var scores = await _bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.TOP);
                                ShowScores(scores, RequestParameters.ScoreType.TOP).Add(_bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
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
                                var scores = await _bot._scoreSaberClient.GetScoresById(id, RequestParameters.ScoreType.RECENT);
                                ShowScores(scores, RequestParameters.ScoreType.RECENT).Add(_bot._watcher, ctx);
                            }
                            catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`An internal server exception occured. Please retry later.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                cancellationTokenSource.Cancel();
                                ctx.Client.ComponentInteractionCreated -= RunInteraction;
                                return;
                            }
                            catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                            {
                                embed.Author.IconUrl = Resources.LogIcons.Error;
                                embed.Color = ColorHelper.Error;
                                embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
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
                            ShowProfile().Add(_bot._watcher, ctx);
                        }

                        cancellationTokenSource.Cancel();
                        cancellationTokenSource = new();

                        try
                        {
                            await Task.Delay(120000, cancellationTokenSource.Token);
                            embed.Footer.Text += " • Interaction timed out";
                            await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(OpenProfileInBrowser));

                            ctx.Client.ComponentInteractionCreated -= RunInteraction;
                        }
                        catch { }
                    }
                }).Add(_bot._watcher, ctx);
            }

            async Task ShowScores(PlayerScores scores, RequestParameters.ScoreType scoreType)
            {
                embed.ClearFields();
                embed.ImageUrl = "";
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n\n" +
                                    $"{(scoreType == RequestParameters.ScoreType.TOP ? "**Top Scores**" : "**Recent Scores**")}";

                foreach (var score in scores.playerScores.Take(5))
                {
                    embed.AddField(new DiscordEmbedField($"{score.leaderboard.songName}{(!string.IsNullOrWhiteSpace(score.leaderboard.songSubName) ? $" {score.leaderboard.songSubName}" : "")} - {score.leaderboard.songAuthorName} [{score.leaderboard.levelAuthorName}]".TruncateWithIndication(256),
                        $":globe_with_meridians: **#{score.score.rank}**  󠂪 󠂪| 󠂪 󠂪 {Formatter.Timestamp(score.score.timeSet, TimestampFormat.RelativeTime)}\n" +
                        $"{(score.leaderboard.ranked ? $"**`{((decimal)((decimal)score.score.modifiedScore / (decimal)score.leaderboard.maxScore) * 100).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}%`**󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪**`{(score.score.pp).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp [{(score.score.pp * score.score.weight).ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp]`**\n" : "\n")}" +
                        $"`{score.score.modifiedScore.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}` 󠂪 󠂪| 󠂪 󠂪 **{(score.score.fullCombo ? ":white_check_mark: `FC`" : $"{false.BoolToEmote()} `{score.score.missedNotes + score.score.badCuts}`")}**"));
                }

                DiscordMessageBuilder builder = new();

                if (_bot._users.List[ctx.User.Id].ScoreSaber.Id == 0 && AddLinkButton)
                    builder.AddComponents(LinkButton);

                _ = msg.ModifyAsync(builder.WithEmbed(embed).AddComponents((scoreType == RequestParameters.ScoreType.TOP ? TopScoreInteractionRow : RecentScoreInteractionRow)));
            }

            string LoadedGraph = "";

            async Task ShowProfile()
            {
                embed.ClearFields();
                embed.Title = $"{player.name} 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪`{player.pp.ToString().Replace(",", ".")}pp`";
                embed.Color = ColorHelper.Info;
                embed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.profilePicture };
                embed.Description = $":globe_with_meridians: **#{player.rank}** 󠂪 󠂪 󠂪| 󠂪 󠂪 󠂪:flag_{player.country.ToLower()}: **#{player.countryRank}**\n";
                embed.AddField(new DiscordEmbedField("Ranked Play Count", $"`{player.scoreStats.rankedPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Ranked Score", $"`{player.scoreStats.totalRankedScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Average Ranked Accuracy", $"`{Math.Round(player.scoreStats.averageRankedAccuracy, 2).ToString().Replace(",", ".")}%`", true));
                embed.AddField(new DiscordEmbedField("Total Play Count", $"`{player.scoreStats.totalPlayCount}`", true));
                embed.AddField(new DiscordEmbedField("Total Score", $"`{player.scoreStats.totalScore.ToString("N", CultureInfo.GetCultureInfo("en-US")).Replace(".000", "")}`", true));
                embed.AddField(new DiscordEmbedField("Replays Watched By Others", $"`{player.scoreStats.replaysWatched}`", true));

                DiscordMessageBuilder builder = new();

                if (_bot._users.List[ctx.User.Id].ScoreSaber.Id == 0 && AddLinkButton)
                    builder.AddComponents(LinkButton);

                if (!string.IsNullOrWhiteSpace(LoadedGraph))
                {
                    embed.Author.IconUrl = ctx.Guild.IconUrl;
                    embed.ImageUrl = LoadedGraph;
                    builder.AddComponents(ProfileInteractionRow);
                }

                msg.ModifyAsync(builder.WithEmbed(embed)).Add(_bot._watcher, ctx);

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
                            var asset = await (await ctx.Client.GetChannelAsync(945747744302174258)).SendMessageAsync(new DiscordMessageBuilder().WithFile(file, stream));

                            LoadedGraph = asset.Attachments[0].Url;

                            embed.Author.IconUrl = ctx.Guild.IconUrl;
                            embed.ImageUrl = asset.Attachments[0].Url;
                            builder = builder.WithEmbed(embed);
                            builder.AddComponents(ProfileInteractionRow);
                            _ = msg.ModifyAsync(builder);
                        }
                    }
                    catch (Exception ex)
                    {
                        embed.Author.IconUrl = ctx.Guild.IconUrl;
                        builder.AddComponents(ProfileInteractionRow);
                        _ = msg.ModifyAsync(builder);
                        LogError(ex.ToString());
                    }

                try
                {
                    await Task.Delay(1000);
                    File.Delete(file);
                }
                catch { }
            }

            ShowProfile().Add(_bot._watcher, ctx);

            ctx.Client.ComponentInteractionCreated += RunInteraction;

            try
            {
                await Task.Delay(120000, cancellationTokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(OpenProfileInBrowser));

                ctx.Client.ComponentInteractionCreated -= RunInteraction;
            }
            catch { }
        }
        catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`An internal server exception occured. Please retry later.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`The access to the player api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`Couldn't find the specified player.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Xorog.ScoreSaber.Exceptions.UnprocessableEntity)
        {
            embed.Author.IconUrl = Resources.LogIcons.Error;
            embed.Color = ColorHelper.Error;
            embed.Description = $"`Please provide an user id.`";
            _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
        }
        catch (Exception)
        {
            _ = msg.DeleteAsync();
            throw;
        }
    }



    [Command("scoresaber"), Aliases("ss"),
    CommandModule("scoresaber"),
    Description("Get show a users Score Saber profile by id")]
    public async Task ScoreSaberC(CommandContext ctx, [Description("Id|@User")] string id = "")
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

            bool AddLinkButton = true;

            if ((string.IsNullOrWhiteSpace(id) || id.Contains('@')) && ctx.Message.MentionedUsers != null && ctx.Message.MentionedUsers.Count > 0)
            {
                if (id.Contains('@'))
                    if (!_bot._users.List.ContainsKey(ctx.Message.MentionedUsers[0].Id))
                        _bot._users.List.Add(ctx.Message.MentionedUsers[0].Id, new Users.Info(_bot));

                if (_bot._users.List[ctx.Message.MentionedUsers[0].Id].ScoreSaber.Id != 0)
                {
                    id = _bot._users.List[ctx.Message.MentionedUsers[0].Id].ScoreSaber.Id.ToString();
                    AddLinkButton = false;
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = Resources.LogIcons.Error, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                        Color = ColorHelper.Error,
                        Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                        Timestamp = DateTime.UtcNow,
                        Description = $"`This user has no Score Saber Profile linked to their Discord Account.`"
                    };

                    _ = ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    return;
                }
            }

            await SendScoreSaberProfile(ctx, id, AddLinkButton);
        }).Add(_bot._watcher, ctx);
    }



    [Command("scoresaber-search"), Aliases("sss", "scoresabersearch"),
    CommandModule("scoresaber"),
    Description("Search a user on Score Saber by name")]
    public async Task ScoreSaberSearch(CommandContext ctx, [Description("Name")][RemainingText] string name)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

            DiscordSelectComponent GetContinents(string default_code)
            {
                List<DiscordSelectComponentOption> continents = new();
                continents.Add(new DiscordSelectComponentOption($"No country filter (may load much longer)", "no_country", "", (default_code == "no_country")));
                foreach (var b in _bot._countryCodes.List.GroupBy(x => x.Value.ContinentCode).Select(x => x.First()).Take(24))
                {
                    continents.Add(new DiscordSelectComponentOption($"{b.Value.ContinentName}", b.Value.ContinentCode, "", (default_code == b.Value.ContinentCode)));
                }
                return new DiscordSelectComponent("continent_selection", "Select a continent..", continents as IEnumerable<DiscordSelectComponentOption>);
            }

            DiscordSelectComponent GetCountries(string continent_code, string default_country, int page)
            {
                List<DiscordSelectComponentOption> countries = new();
                var currentCountryList = _bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == continent_code.ToLower()).Skip((page - 1) * 25).Take(25).ToList();

                foreach (var b in currentCountryList)
                {
                    DiscordEmoji flag_emote = null;
                    try
                    { flag_emote = DiscordEmoji.FromName(ctx.Client, $":flag_{b.Key.ToLower()}:"); }
                    catch (Exception) { flag_emote = DiscordEmoji.FromName(ctx.Client, $":white_large_square:"); }
                    countries.Add(new DiscordSelectComponentOption($"{b.Value.Name}", b.Key, "", (b.Key == default_country), new DiscordComponentEmoji(flag_emote)));
                }
                return new DiscordSelectComponent("country_selection", "Select a country..", countries as IEnumerable<DiscordSelectComponentOption>);
            }

            var start_search_button = new DiscordButtonComponent(ButtonStyle.Success, "start_search", "Start Search", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":mag:")));
            var next_step_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_step", "Next step", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

            var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
            var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Search • {ctx.Guild.Name}" },
                Color = ColorHelper.AwaitingInput,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                Timestamp = DateTime.UtcNow,
                Description = $"`Please select a continent filter below.`"
            };

            var msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents("no_country")).AddComponents(start_search_button));
            CancellationTokenSource tokenSource = new();

            string selectedContinent = "no_country";
            string selectedCountry = "no_country";
            int lastFetchedPage = -1;
            int currentPage = 1;
            int currentFetchedPage = 1;
            bool playerSelection = false;
            PlayerSearch.SearchResult lastSearch = null;

            async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (e.Message.Id == msg.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            async Task RefreshCountryList()
                            {
                                embed.Description = "`Please select a country filter below.`";

                                if (selectedCountry != "no_country")
                                {
                                    embed.Description += $"\n`Selected country: '{_bot._countryCodes.List[selectedCountry].Name}'`";
                                }

                                var page = GetCountries(selectedContinent, selectedCountry, currentPage);
                                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(page);

                                if (currentPage == 1 && _bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Count() > 25)
                                {
                                    builder.AddComponents(next_page_button);
                                }

                                if (currentPage != 1)
                                {
                                    if (_bot._countryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Skip((currentPage - 1) * 25).Count() > 25)
                                        builder.AddComponents(next_page_button);

                                    builder.AddComponents(previous_page_button);
                                }

                                if (selectedCountry != "no_country")
                                    builder.AddComponents(start_search_button);

                                msg.ModifyAsync(builder).Add(_bot._watcher, ctx);
                            }

                            async Task RefreshPlayerList()
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                embed.Description = "`Searching for players with specified criteria..`";
                                embed.Author.IconUrl = Resources.StatusIndicators.DiscordCircleLoading;
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));

                                if (currentFetchedPage != lastFetchedPage)
                                {
                                    try
                                    {
                                        lastSearch = await _bot._scoreSaberClient.SearchPlayer(name, currentFetchedPage, (selectedCountry != "no_country" ? selectedCountry : ""));
                                    }
                                    catch (Xorog.ScoreSaber.Exceptions.InternalServerError)
                                    {
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        embed.Color = ColorHelper.Error;
                                        embed.Description = $"`An internal server exception occured. Please retry later.`";
                                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                        return;
                                    }
                                    catch (Xorog.ScoreSaber.Exceptions.ForbiddenException)
                                    {
                                        embed.Author.IconUrl = Resources.LogIcons.Error;
                                        embed.Color = ColorHelper.Error;
                                        embed.Description = $"`The access to the search api endpoint is currently forbidden. This may mean that it's temporarily disabled.`";
                                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    lastFetchedPage = currentFetchedPage;
                                }

                                List<DiscordSelectComponentOption> playerDropDownOptions = new();
                                var playerList = lastSearch.players.Skip((currentPage - 1) * 25).Take(25).ToList();
                                foreach (var b in playerList)
                                {
                                    playerDropDownOptions.Add(new DiscordSelectComponentOption($"{b.name} | {b.pp.ToString().Replace(",", ".")}pp", b.id, $"🌐 #{b.rank} | {b.country.IsoCountryCodeToFlagEmoji()} #{b.countryRank}"));
                                }
                                var player_dropdown = new DiscordSelectComponent("player_selection", "Select a player..", playerDropDownOptions as IEnumerable<DiscordSelectComponentOption>);

                                var builder = new DiscordMessageBuilder().AddComponents(player_dropdown);

                                bool added_next = false;

                                if (currentPage == 1 && lastSearch.players.Length > 25)
                                {
                                    builder.AddComponents(next_page_button);
                                    added_next = true;
                                }

                                if (currentPage != 1 || lastFetchedPage != 1)
                                {
                                    if ((lastSearch.players.Skip((currentPage - 1) * 25).Take(25).Count() > 25 || ((((lastSearch.metadata.total - (currentFetchedPage - 1)) * 50) > 0) && player_dropdown.Options.Count == 25)) && !added_next)
                                        builder.AddComponents(next_page_button);

                                    builder.AddComponents(previous_page_button);
                                }

                                ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                embed.Description = $"`Found {lastSearch.metadata.total} players. Fetched {lastSearch.players.Length} players. Showing {playerDropDownOptions.Count} players.`";
                                embed.Author.IconUrl = ctx.Guild.IconUrl;
                                builder.WithEmbed(embed);
                                await msg.ModifyAsync(builder);
                            }

                            if (e.Interaction.Data.CustomId == "start_search")
                            {
                                tokenSource.Cancel();
                                tokenSource = null;

                                playerSelection = true;
                                currentPage = 1;
                                await RefreshPlayerList();

                                tokenSource = new();
                            }
                            else if (e.Interaction.Data.CustomId == "player_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                tokenSource.Cancel();
                                tokenSource = null;

                                _ = SendScoreSaberProfile(ctx, e.Values.First());
                                _ = msg.DeleteAsync();

                                return;
                            }
                            else if (e.Interaction.Data.CustomId == "next_step")
                            {
                                _ = RefreshCountryList();
                            }
                            else if (e.Interaction.Data.CustomId == "country_selection")
                            {
                                selectedCountry = e.Values.First();

                                _ = RefreshCountryList();
                            }
                            else if (e.Interaction.Data.CustomId == "prev_page")
                            {
                                if (playerSelection)
                                {
                                    if (currentPage == 1)
                                    {
                                        currentPage = 2;
                                        currentFetchedPage -= 1;
                                    }
                                    else
                                        currentPage -= 1;

                                    tokenSource.Cancel();
                                    tokenSource = null;

                                    await RefreshPlayerList();

                                    tokenSource = new();
                                }
                                else
                                {
                                    currentPage -= 1;
                                    _ = RefreshCountryList();
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "next_page")
                            {
                                if (playerSelection)
                                {
                                    if (currentPage == 2)
                                    {
                                        currentPage = 1;
                                        currentFetchedPage += 1;
                                    }
                                    else
                                        currentPage += 1;

                                    tokenSource.Cancel();
                                    tokenSource = null;

                                    await RefreshPlayerList();

                                    tokenSource = new();
                                }
                                else
                                {
                                    currentPage += 1;
                                    _ = RefreshCountryList();
                                }
                            }
                            else if (e.Interaction.Data.CustomId == "continent_selection")
                            {
                                selectedContinent = e.Values.First();

                                if (selectedContinent != "no_country")
                                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(next_step_button));
                                else
                                    _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(start_search_button));
                            }

                            try
                            {
                                tokenSource.Cancel();
                                tokenSource = new();
                                await Task.Delay(120000, tokenSource.Token);
                                embed.Footer.Text += " • Interaction timed out";
                                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                                await Task.Delay(5000);
                                _ = msg.DeleteAsync();

                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                            }
                            catch { }
                        }
                    }
                    catch (Xorog.ScoreSaber.Exceptions.NotFoundException)
                    {
                        embed.Author.IconUrl = Resources.LogIcons.Error;
                        embed.Color = ColorHelper.Error;
                        embed.Description = $"`Couldn't find any player with the specified criteria.`";
                        _ = msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                    }
                    catch (Exception)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                        throw;
                    }
                }).Add(_bot._watcher, ctx);
            }
            ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

            try
            {
                await Task.Delay(120000, tokenSource.Token);
                embed.Footer.Text += " • Interaction timed out";
                await msg.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embed));
                await Task.Delay(5000);
                _ = msg.DeleteAsync();

                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
            }
            catch { }

            return;
        }).Add(_bot._watcher, ctx);
    }



    //[Command("scoresaber-map-leaderboard"), Aliases("ssml", "scoresabermapleaderboard"),
    //CommandModule("scoresaber"),
    //Description("Display a leaderboard off a specific map")]
    //public async Task ScoreSaberMapLeaderboard(CommandContext ctx, [Description("LeaderboardId")][RemainingText] string boardId)
    //{
    //    Task.Run(async () =>
    //    {

    //    }).Add(_bot._watcher, ctx);
    //}



    [Command("scoresaber-unlink"), Aliases("ssu", "scoresaberunlink"),
    CommandModule("scoresaber"),
    Description("Unlink your Score Saber Profile from your Discord Account")]
    public async Task ScoreSaberUnlink(CommandContext ctx)
    {
        Task.Run(async () =>
        {
            if (await _bot._users.List[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx.Message))
                return;

            if (!_bot._users.List.ContainsKey(ctx.User.Id))
                _bot._users.List.Add(ctx.User.Id, new Users.Info(_bot));

            if (_bot._users.List[ctx.User.Id].ScoreSaber.Id != 0)
            {
                _bot._users.List[ctx.User.Id].ScoreSaber.Id = 0;

                var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"{ctx.User.Mention} `Unlinked your Score Saber Profile from your Discord Account`"
                }));

                _ = Task.Delay(10000).ContinueWith(x =>
                {
                    _ = new_msg.DeleteAsync();
                });
            }
            else
            {
                var new_msg = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Guild.IconUrl, Name = $"Score Saber Profile • {ctx.Guild.Name}" },
                    Color = ColorHelper.Error,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.Member.AvatarUrl, Text = $"This message automatically deletes in 10 seconds • Command used by {ctx.Member.Username}#{ctx.Member.Discriminator}" },
                    Timestamp = DateTime.UtcNow,
                    Description = $"{ctx.User.Mention} `There is no Score Saber Profile linked to your Discord Account.`"
                }));

                _ = Task.Delay(10000).ContinueWith(x =>
                {
                    _ = new_msg.DeleteAsync();
                });
            }
        }).Add(_bot._watcher, ctx);
    }
}
