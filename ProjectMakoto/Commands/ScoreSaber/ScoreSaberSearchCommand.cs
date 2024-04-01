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

internal sealed class ScoreSaberSearchCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var name = (string)arguments["name"];

            if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
                return;

            DiscordStringSelectComponent GetContinents(string default_code)
            {
                List<DiscordStringSelectComponentOption> continents = new() { new DiscordStringSelectComponentOption(this.GetString(this.t.Commands.ScoreSaber.Search.NoCountryFilter), "no_country", "", (default_code == "no_country")) };
                foreach (var b in ctx.Bot.CountryCodes.List.GroupBy(x => x.Value.ContinentCode).Select(x => x.First()).Take(24))
                {
                    continents.Add(new DiscordStringSelectComponentOption($"{b.Value.ContinentName}", b.Value.ContinentCode, "", (default_code == b.Value.ContinentCode)));
                }
                return new DiscordStringSelectComponent(this.GetString(this.t.Commands.ScoreSaber.Search.SelectContinentDropdown), continents as IEnumerable<DiscordStringSelectComponentOption>, "continent_selection");
            }

            DiscordStringSelectComponent GetCountries(string continent_code, string default_country, int page)
            {
                List<DiscordStringSelectComponentOption> countries = new();
                var currentCountryList = ctx.Bot.CountryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == continent_code.ToLower()).Skip((page - 1) * 25).Take(25).ToList();

                foreach (var b in currentCountryList)
                {
                    DiscordEmoji flag_emote = null;
                    try
                    { flag_emote = DiscordEmoji.FromName(ctx.Client, $":flag_{b.Key.ToLower()}:"); }
                    catch (Exception) { flag_emote = DiscordEmoji.FromUnicode("⬜"); }
                    countries.Add(new DiscordStringSelectComponentOption($"{b.Value.Name}", b.Key, "", (b.Key == default_country), new DiscordComponentEmoji(flag_emote)));
                }
                return new DiscordStringSelectComponent(this.GetString(this.t.Commands.ScoreSaber.Search.SelectCountryDropdown), countries as IEnumerable<DiscordStringSelectComponentOption>, "country_selection");
            }

            var start_search_button = new DiscordButtonComponent(ButtonStyle.Success, "start_search", this.GetString(this.t.Commands.ScoreSaber.Search.StartSearch), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔎")));
            var next_step_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_step", this.GetString(this.t.Commands.ScoreSaber.Search.NextStep), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

            var previous_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "prev_page", this.GetString(this.t.Common.PreviousPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
            var next_page_button = new DiscordButtonComponent(ButtonStyle.Primary, "next_page", this.GetString(this.t.Common.NextPage), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

            var embed = new DiscordEmbedBuilder
            {
                Description = this.GetString(this.t.Commands.ScoreSaber.Search.SelectContinent, true)
            }.AsAwaitingInput(ctx, "Score Saber");

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents("no_country")).AddComponents(start_search_button));
            CancellationTokenSource tokenSource = new();

            var selectedContinent = "no_country";
            var selectedCountry = "no_country";
            var lastFetchedPage = -1;
            var currentPage = 1;
            var currentFetchedPage = 1;
            var playerSelection = false;
            PlayerSearch lastSearch = null;

            async Task RunDropdownInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (e.Message?.Id == ctx.ResponseMessage.Id && e.User.Id == ctx.User.Id)
                        {
                            _ = e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                            async Task RefreshCountryList()
                            {
                                embed.Description = this.GetString(this.t.Commands.ScoreSaber.Search.SelectCountry, true);

                                if (selectedCountry != "no_country")
                                {
                                    embed.Description += $"\n`{this.GetString(this.t.Commands.ScoreSaber.Search.SelectedCountry)}: '{ctx.Bot.CountryCodes.List[selectedCountry].Name}'`";
                                }

                                var page = GetCountries(selectedContinent, selectedCountry, currentPage);
                                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(page);

                                if (currentPage == 1 && ctx.Bot.CountryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Count() > 25)
                                {
                                    _ = builder.AddComponents(next_page_button);
                                }

                                if (currentPage != 1)
                                {
                                    if (ctx.Bot.CountryCodes.List.Where(x => x.Value.ContinentCode.ToLower() == selectedContinent.ToLower()).Skip((currentPage - 1) * 25).Count() > 25)
                                        _ = builder.AddComponents(next_page_button);

                                    _ = builder.AddComponents(previous_page_button);
                                }

                                if (selectedCountry != "no_country")
                                    _ = builder.AddComponents(start_search_button);

                                _ = await this.RespondOrEdit(builder);
                            }

                            async Task RefreshPlayerList()
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                embed.Description = this.GetString(this.t.Commands.ScoreSaber.Search.Searching, true);
                                _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsLoading(ctx, "Score Saber")));

                                if (currentFetchedPage != lastFetchedPage)
                                {
                                    try
                                    {
                                        lastSearch = await ctx.Bot.ScoreSaberClient.SearchPlayer(name, currentFetchedPage, (selectedCountry != "no_country" ? selectedCountry : ""));
                                    }
                                    catch (InternalServerErrorException)
                                    {
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                                        embed.Description = this.GetString(this.t.Commands.ScoreSaber.InternalServerError, true);
                                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                                        return;
                                    }
                                    catch (ForbiddenException)
                                    {
                                        tokenSource.Cancel();
                                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;

                                        embed.Description = this.GetString(this.t.Commands.ScoreSaber.ForbiddenError, true);
                                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                                        return;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    lastFetchedPage = currentFetchedPage;
                                }

                                List<DiscordStringSelectComponentOption> playerDropDownOptions = new();
                                var playerList = lastSearch.players.Skip((currentPage - 1) * 25).Take(25).ToList();
                                foreach (var b in playerList)
                                {
                                    playerDropDownOptions.Add(new DiscordStringSelectComponentOption($"{b.name.FullSanitize()} | {b.pp.ToString("N2", CultureInfo.CreateSpecificCulture("en-US"))}pp", b.id, $"🌐 #{b.rank} | {b.country.IsoCountryCodeToFlagEmoji()} #{b.countryRank}"));
                                }
                                var player_dropdown = new DiscordStringSelectComponent(this.GetString(this.t.Commands.ScoreSaber.Search.SelectPlayer), playerDropDownOptions as IEnumerable<DiscordStringSelectComponentOption>, "player_selection");

                                var builder = new DiscordMessageBuilder().AddComponents(player_dropdown);

                                var added_next = false;

                                if (currentPage == 1 && lastSearch.players.Length > 25)
                                {
                                    _ = builder.AddComponents(next_page_button);
                                    added_next = true;
                                }

                                if (currentPage != 1 || lastFetchedPage != 1)
                                {
                                    if ((lastSearch.players.Skip((currentPage - 1) * 25).Take(25).Count() > 25 || ((((lastSearch.metadata.total - (currentFetchedPage - 1)) * 50) > 0) && player_dropdown.Options.Count == 25)) && !added_next)
                                        _ = builder.AddComponents(next_page_button);

                                    _ = builder.AddComponents(previous_page_button);
                                }

                                ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

                                embed.Description = this.GetString(this.t.Commands.ScoreSaber.Search.FoundCount, true, new TVar("TotalCount", lastSearch.metadata.total));
                                _ = await this.RespondOrEdit(builder.WithEmbed(embed.AsSuccess(ctx, "Score Saber")));
                            }

                            if (e.GetCustomId() == "start_search")
                            {
                                tokenSource.Cancel();
                                tokenSource = null;

                                playerSelection = true;
                                currentPage = 1;
                                await RefreshPlayerList();

                                tokenSource = new();
                            }
                            else if (e.GetCustomId() == "player_selection")
                            {
                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                                tokenSource.Cancel();
                                tokenSource = null;

                                await ScoreSaberCommandAbstractions.SendScoreSaberProfile(ctx, e.Values.First());
                                return;
                            }
                            else if (e.GetCustomId() == "next_step")
                            {
                                _ = RefreshCountryList();
                            }
                            else if (e.GetCustomId() == "country_selection")
                            {
                                selectedCountry = e.Values.First();

                                _ = RefreshCountryList();
                            }
                            else if (e.GetCustomId() == "prev_page")
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
                            else if (e.GetCustomId() == "next_page")
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
                            else if (e.GetCustomId() == "continent_selection")
                            {
                                selectedContinent = e.Values.First();

                                _ = selectedContinent != "no_country"
                                    ? await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(next_step_button))
                                    : await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed).AddComponents(GetContinents(selectedContinent)).AddComponents(start_search_button));
                            }

                            try
                            {
                                tokenSource.Cancel();
                                tokenSource = new();
                                await Task.Delay(120000, tokenSource.Token);
                                this.ModifyToTimedOut();

                                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                            }
                            catch { }
                        }
                    }
                    catch (NotFoundException)
                    {
                        embed.Description = this.GetString(this.t.Commands.ScoreSaber.Search.NoSearchResult, true);
                        _ = await this.RespondOrEdit(new DiscordMessageBuilder().WithEmbed(embed.AsError(ctx, "Score Saber")));
                    }
                    catch (Exception)
                    {
                        ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
                        throw;
                    }
                }).Add(ctx.Bot, ctx);
            }
            ctx.Client.ComponentInteractionCreated += RunDropdownInteraction;

            try
            {
                await Task.Delay(120000, tokenSource.Token);
                this.ModifyToTimedOut();

                ctx.Client.ComponentInteractionCreated -= RunDropdownInteraction;
            }
            catch { }
        });
    }
}