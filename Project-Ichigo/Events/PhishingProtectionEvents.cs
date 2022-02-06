namespace Project_Ichigo.Events;

internal class PhishingProtectionEvents
{
    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = CheckMessage(sender, e.Guild, e.Guild.Members[e.Message.Author.Id], e.Message);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        _ = CheckMessage(sender, e.Guild, e.Guild.Members[e.Message.Author.Id], e.Message);
    }

    private async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMember member, DiscordMessage e)
    {
        foreach (var url in Bot._phishingUrls.List)
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
                        foreach (var url in Bot._phishingUrls.List)
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
        if (!Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.DetectPhishing)
            return;

        switch (Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.PunishmentType)
        {
            case Settings.PhishingPunishmentType.DELETE:
            {
                _ = e.DeleteAsync();
                break;
            }
            case Settings.PhishingPunishmentType.TIMEOUT:
            {
                _ = e.DeleteAsync();
                _ = member.TimeoutAsync(Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentLength, Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case Settings.PhishingPunishmentType.KICK:
            {
                _ = e.DeleteAsync();
                _ = member.RemoveAsync(Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
            case Settings.PhishingPunishmentType.BAN:
            {
                _ = e.DeleteAsync();
                _ = member.BanAsync(7, Bot._guilds.Servers[guild.Id].PhishingDetectionSettings.CustomPunishmentReason.Replace("%R", $"Detected Malicous Url [{url}]"));
                break;
            }
        }
    }

    private Dictionary<string, DateTime> recentlyResolvedUrls = new();
}
