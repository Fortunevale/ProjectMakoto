namespace Project_Ichigo.Events;

internal class PhishingProtectionEvents
{
    internal PhishingProtectionEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }



    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(_bot._watcher);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore.Content != e.Message.Content)
            CheckMessage(sender, e.Guild, e.Message).Add(_bot._watcher);
    }

    private async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        if (e.Content.StartsWith($";;"))
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($";;{command.Key}") || e.Content.StartsWith($">>{command.Key}"))
                    return;

        if (e.WebhookMessage || guild is null)
            return;

        if (!_bot._guilds.List.ContainsKey(guild.Id))
            _bot._guilds.List.Add(guild.Id, new Guilds.ServerSettings());

        if (!_bot._guilds.List[guild.Id].PhishingDetectionSettings.DetectPhishing)
            return;

        DiscordMember member;

        try
        {
            member = await guild.GetMemberAsync(e.Author.Id);
        }
        catch (Exception)
        {
            throw;
        }

        var matches = Regex.Matches(e.Content, RegexHelper.UrlRegex);
        var parsedMatches = matches.Select(x => new UriBuilder(x.Value));

        var parsedWords = e.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var url in _bot._phishingUrls.List)
        {
            foreach (var word in parsedWords)
            {
                if (word.ToLower() == url.Key.ToLower())
                {
                    _ = PunishMember(guild, member, e, url.Key);
                    return;
                }
            }
        }

        foreach (var url in _bot._phishingUrls.List)
        {
            foreach (var match in parsedMatches)
            {
                if (match.Host.ToLower() == url.Key.ToLower())
                {
                    _ = PunishMember(guild, member, e, url.Key);
                    return;
                }
            }
        }

        if (matches.Count() > 0)
        {
            Dictionary<string, string> redirectUrls = new();

            foreach (Match match in matches)
            {
                try
                {
                    var unshortened_url = await UnshortenUrl(match.Value);
                    var parsedUri = new UriBuilder(unshortened_url);

                    if (unshortened_url != match.Value)
                    {
                        foreach (var url in _bot._phishingUrls.List)
                        {
                            if (parsedUri.Host.ToLower() == url.Key.ToLower())
                            {
                                _ = PunishMember(guild, member, e, url.Key);
                                return;
                            }
                        }

                        if (!recentlyResolvedUrls.ContainsKey(unshortened_url) || recentlyResolvedUrls[unshortened_url].AddSeconds(10) < DateTime.UtcNow)
                            redirectUrls.Add(match.Value, unshortened_url);
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message.Contains("Cannot write more bytes"))
                    {
                        if (_bot._guilds.List[guild.Id].PhishingDetectionSettings.WarnOnRedirect)
                            _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                            {
                                Title = $":no_entry: Couldn't check this link for malicous redirects. Please proceed with caution.",
                                Color = ColorHelper.Error
                            });
                    }
                }
                catch (Exception ex)
                {
                    LogError($"An exception occured while trying to unshorten url '{match}'", ex);

                    if (_bot._guilds.List[guild.Id].PhishingDetectionSettings.WarnOnRedirect)
                        _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $":no_entry: An unknown error occured while trying to check for malicous redirects. Please proceed with caution.",
                            Color = ColorHelper.Error
                        });
                }
            }

            if (redirectUrls.Count > 0)
            {
                foreach (var b in redirectUrls)
                    if (!recentlyResolvedUrls.ContainsKey(b.Value))
                        recentlyResolvedUrls.Add(b.Value, DateTime.UtcNow);
                    else
                        recentlyResolvedUrls[b.Value] = DateTime.UtcNow;

                if (_bot._guilds.List[guild.Id].PhishingDetectionSettings.WarnOnRedirect)
                    _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = $":warning: Found at least one (or more) redirected URLs in this message.",
                        Description = $"`{string.Join("`\n`", redirectUrls.Select(x => x.Value))}`",
                        Color = ColorHelper.Warning
                    });
            }
        }
    }

    private async Task PunishMember(DiscordGuild guild, DiscordMember member, DiscordMessage e, string url)
    {
        if (!_bot._guilds.List[guild.Id].PhishingDetectionSettings.DetectPhishing)
            return;

        switch (_bot._guilds.List[guild.Id].PhishingDetectionSettings.PunishmentType)
        {
            case PhishingPunishmentType.DELETE:
            {
                _ = e.DeleteAsync();
                break;
            }
            case PhishingPunishmentType.TIMEOUT:
            {
                _ = e.DeleteAsync();
                _ = member.TimeoutAsync(_bot._guilds.List[guild.Id].PhishingDetectionSettings.CustomPunishmentLength, _bot._guilds.List[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case PhishingPunishmentType.KICK:
            {
                _ = e.DeleteAsync();
                _ = member.RemoveAsync(_bot._guilds.List[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case PhishingPunishmentType.BAN:
            {
                _ = e.DeleteAsync();
                _ = member.BanAsync(7, _bot._guilds.List[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
        }
    }

    private Dictionary<string, DateTime> recentlyResolvedUrls = new();
}
