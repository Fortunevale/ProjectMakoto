namespace Project_Ichigo.Events;

internal class PhishingProtectionEvents
{
    internal PhishingProtectionEvents(PhishingUrls _phishingUrls, ServerInfo _guilds, TaskWatcher.TaskWatcher watcher)
    {
        this._phishingUrls = _phishingUrls;
        this._guilds = _guilds;
        this._watcher = watcher;
    }

    internal PhishingUrls _phishingUrls { private get; set; }
    public ServerInfo _guilds { private get; set; }
    TaskWatcher.TaskWatcher _watcher { get; set; }



    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(_watcher);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        CheckMessage(sender, e.Guild, e.Message).Add(_watcher);
    }

    private async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        if (e.Content.StartsWith($"-"))
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($"-{command.Key}"))
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

        foreach (var url in _phishingUrls.List)
        {
            if (e.Content.Contains(url.Key))
            {
                _ = PunishMember(guild, member, e, url.Key);
                return;
            }
        }

        var matches = Regex.Matches(e.Content, RegexHelper.UrlRegex);

        if (matches.Count > 0)
        {
            Dictionary<string, string> redirectUrls = new();

            foreach (Match match in matches)
            {
                try
                {
                    var unshortened_url = await UnshortenUrl(match.Value);

                    if (unshortened_url != match.Value)
                    {
                        foreach (var url in _phishingUrls.List)
                        {
                            if (unshortened_url.Contains(url.Key))
                            {
                                _ = PunishMember(guild, member, e, url.Key);
                                return;
                            }
                        }

                        if (!recentlyResolvedUrls.ContainsKey(unshortened_url) || recentlyResolvedUrls[unshortened_url].AddSeconds(10) < DateTime.UtcNow)
                            redirectUrls.Add(match.Value, unshortened_url);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"An exception occured while trying to unshorten url '{match.Value}': {ex}");
                }
            }

            if (redirectUrls.Count > 0)
            {
                foreach (var b in redirectUrls)
                    if (!recentlyResolvedUrls.ContainsKey(b.Value))
                        recentlyResolvedUrls.Add(b.Value, DateTime.UtcNow);
                    else
                        recentlyResolvedUrls[b.Value] = DateTime.UtcNow;

                _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = $":warning: Found at least one (or more) redirected URLs in this message.",
                    Description = $"`{string.Join("`\n`", redirectUrls.Select(x => x.Value))}`",
                    Color = DiscordColor.Orange
                });
            }
        }
    }

    private async Task PunishMember(DiscordGuild guild, DiscordMember member, DiscordMessage e, string url)
    {
        if (!_guilds.Servers[guild.Id].PhishingDetectionSettings.DetectPhishing)
            return;

        switch (_guilds.Servers[guild.Id].PhishingDetectionSettings.PunishmentType)
        {
            case PhishingPunishmentType.DELETE:
            {
                _ = e.DeleteAsync();
                break;
            }
            case PhishingPunishmentType.TIMEOUT:
            {
                _ = e.DeleteAsync();
                _ = member.TimeoutAsync(_guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentLength, _guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case PhishingPunishmentType.KICK:
            {
                _ = e.DeleteAsync();
                _ = member.RemoveAsync(_guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case PhishingPunishmentType.BAN:
            {
                _ = e.DeleteAsync();
                _ = member.BanAsync(7, _guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
        }
    }

    private Dictionary<string, DateTime> recentlyResolvedUrls = new();
}
