// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class PhishingProtectionEvents(Bot bot) : RequiresTranslation(bot)
{
    Translations.events.phishing tKey
        => this.t.Events.Phishing;

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        _ = this.CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    internal async Task MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
    {
        if (e.MessageBefore?.Content != e.Message?.Content)
            _ = this.CheckMessage(sender, e.Guild, e.Message).Add(this.Bot);
    }

    private async Task CheckMessage(DiscordClient sender, DiscordGuild guild, DiscordMessage e)
    {
        var prefix = guild.GetGuildPrefix(this.Bot);

        if (e?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e.WebhookMessage || guild is null || e.Author?.Id == sender.CurrentUser.Id || (e.Author?.IsBot ?? true))
            return;

        if (!this.Bot.Guilds[guild.Id].PhishingDetection.DetectPhishing)
            return;

        var member = await guild.GetMemberAsync(e.Author.Id);

        async Task CheckDb(Uri uri)
        {
            if (!this.Bot.Guilds[guild.Id].PhishingDetection.AbuseIpDbReports)
                return;

            IPAddress[] parsedIp;

            try
            {
                parsedIp = await Dns.GetHostAddressesAsync(uri.Host);
            }
            catch (Exception)
            {
                return;
            }

            var query = await this.Bot.AbuseIpDbClient.QueryIp(parsedIp[0].ToString());

            if (query.data.abuseConfidenceScore.HasValue && query.data.abuseConfidenceScore.Value > 60)
            {
                var report_fields = query.data.reports.Select(x => new DiscordEmbedField($"{x.reporterCountryCode.IsoCountryCodeToFlagEmoji()} {x.reporterId}{(x.reportedAt.HasValue ? $" {x.reportedAt.Value.ToTimestamp()}" : "")}", (x.comment.IsNullOrWhiteSpace() ? "No comment provided." : x.comment).FullSanitize().TruncateWithIndication(1000))).ToList();

                DiscordEmbedBuilder embed = new()
                {
                    Title = this.tKey.AbuseIpDbReport.Get(this.Bot.Guilds[guild.Id]),
                    Description = $"**{this.tKey.HostWasFoundInAbuseIpDb.Get(this.Bot.Guilds[guild.Id]).Build(new TVar("Host", $"`{uri.Host} ({parsedIp[0]})`"))}**\n" +
                                  $"{(query.data.countryName.IsNullOrWhiteSpace() ? "" : $"**{this.tKey.ConfidenceOfAbuse.Get(this.Bot.Guilds[guild.Id])}**: {query.data.abuseConfidenceScore}%\n\n")}" +
                                  $"{(query.data.countryName.IsNullOrWhiteSpace() ? "" : $"**{this.tKey.Country.Get(this.Bot.Guilds[guild.Id])}**: {query.data.countryCode.IsoCountryCodeToFlagEmoji()} {query.data.countryName}\n")}" +
                                  $"{(query.data.isp.IsNullOrWhiteSpace() ? "" : $"**{this.tKey.ISP.Get(this.Bot.Guilds[guild.Id])}**: {query.data.isp}\n")}" +
                                  $"{(query.data.domain.IsNullOrWhiteSpace() ? "" : $"**{this.tKey.DomainName.Get(this.Bot.Guilds[guild.Id])}**: {query.data.domain}\n")}",
                    Color = new DiscordColor("#FF0000"),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = Resources.AbuseIpDbIcon
                    },
                };

                _ = embed.AddFields(report_fields.Take(2));

                _ = e.RespondAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(new DiscordLinkButtonComponent($"https://www.abuseipdb.com/check/{parsedIp[0]}", this.tKey.OpenInBrowser.Get(this.Bot.Guilds[guild.Id]))));
            }
        }

        var matches = RegexTemplates.Url.Matches(e.Content);
        var parsedMatches = matches.Select(x => new UriBuilder(x.Value));

        var parsedWords = e.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var url in this.Bot.PhishingHosts)
        {
            foreach (var word in parsedWords)
            {
                if (word.ToLower() == url.Key.ToLower())
                {
                    _ = this.PunishMember(guild, member, e, url.Key);
                    return;
                }

                var reg = Regex.Match(word.ToLower(), @"([\S]*\.)?([\S]*)\.([\S]*)");

                if (reg.Success && reg.Groups[1].Success)
                {
                    var regex = new Regex(Regex.Escape(reg.Groups[1].Value));

                    if (regex.Replace(word.ToLower(), "", 1) == url.Key.ToLower())
                    {
                        _ = this.PunishMember(guild, member, e, url.Key);
                        return;
                    }
                }
            }
        }

        foreach (var match in parsedMatches)
        {
            if (match.Uri.ToString().Contains('⁄'))
            {
                _ = this.PunishMember(guild, member, e, match.Uri.ToString());
                return;
            }

            _ = CheckDb(match.Uri);
        }

        foreach (var url in this.Bot.PhishingHosts)
        {
            foreach (var match in parsedMatches)
            {
                if (match.Host.ToLower() == url.Key.ToLower())
                {
                    _ = this.PunishMember(guild, member, e, url.Key);
                    return;
                }
            }
        }

        if (matches.Count > 0)
        {
            Dictionary<string, string> redirectUrls = new();

            foreach (var match in matches.Cast<Match>())
            {
                try
                {
                    var unshortenedUrl = await WebTools.UnshortenUrl(match.Value);
                    var parsedUri = new UriBuilder(unshortenedUrl);

                    _ = CheckDb(parsedUri.Uri);

                    if (unshortenedUrl != match.Value)
                    {
                        foreach (var url in this.Bot.PhishingHosts)
                        {
                            if (parsedUri.Host.ToLower() == url.Key.ToLower())
                            {
                                _ = this.PunishMember(guild, member, e, url.Key);
                                return;
                            }
                        }

                        if (!this.recentlyResolvedUrls.TryGetValue(unshortenedUrl, out var value) || value.AddSeconds(10) < DateTime.UtcNow)
                            redirectUrls.Add(match.Value, unshortenedUrl);
                    }
                }
                catch (DepthLimitReachedException)
                {
                    if (this.Bot.Guilds[guild.Id].PhishingDetection.WarnOnRedirect)
                        _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $":no_entry: {this.tKey.RedirectDepthLimitError.Get(this.Bot.Guilds[guild.Id])}",
                            Color = EmbedColors.Error
                        });
                }
                catch (Exception ex) when (ex is TimeoutException ||
                                           (ex is HttpRequestException && ex.Message.Contains("Cannot write more bytes")))
                {
                    if (this.Bot.Guilds[guild.Id].PhishingDetection.WarnOnRedirect)
                        _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $":no_entry: {this.tKey.RedirectCheckTimeoutError.Get(this.Bot.Guilds[guild.Id])}",
                            Color = EmbedColors.Error
                        });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An exception occurred while trying to unshorten url '{url}'", match);

                    if (this.Bot.Guilds[guild.Id].PhishingDetection.WarnOnRedirect)
                        _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $":no_entry: {this.tKey.RedirectCheckTimeoutUnknownError.Get(this.Bot.Guilds[guild.Id])}",
                            Color = EmbedColors.Error
                        });
                }
            }

            if (redirectUrls.Count > 0)
            {
                foreach (var b in redirectUrls)
                    if (!this.recentlyResolvedUrls.ContainsKey(b.Value))
                        this.recentlyResolvedUrls.Add(b.Value, DateTime.UtcNow);
                    else
                        this.recentlyResolvedUrls[b.Value] = DateTime.UtcNow;

                if (this.Bot.Guilds[guild.Id].PhishingDetection.WarnOnRedirect)
                    _ = e.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = $":warning: {this.tKey.FoundRedirects.Get(this.Bot.Guilds[guild.Id])}",
                        Description = $"`{string.Join("`\n`", redirectUrls.Select(x => x.Value))}`",
                        Color = EmbedColors.Warning
                    });
            }
        }
    }

    private async Task PunishMember(DiscordGuild guild, DiscordMember member, DiscordMessage e, string url)
    {
        if (!this.Bot.Guilds[guild.Id].PhishingDetection.DetectPhishing)
            return;

        switch (this.Bot.Guilds[guild.Id].PhishingDetection.PunishmentType)
        {
            case PhishingPunishmentType.Delete:
            {
                _ = e.DeleteAsync();
                break;
            }
            case PhishingPunishmentType.Timeout:
            {
                _ = e.DeleteAsync();
                _ = member.TimeoutAsync(this.Bot.Guilds[guild.Id].PhishingDetection.CustomPunishmentLength, this.Bot.Guilds[guild.Id].PhishingDetection.CustomPunishmentReason.Replace("%R", $"Detected malicious Url [{url}]"));
                break;
            }
            case PhishingPunishmentType.Kick:
            {
                _ = e.DeleteAsync();
                _ = member.RemoveAsync(this.Bot.Guilds[guild.Id].PhishingDetection.CustomPunishmentReason.Replace("%R", this.tKey.DetectedMaliciousHost.Get(this.Bot.Guilds[guild.Id]).Build(new TVar("Host", url))));
                break;
            }
            case PhishingPunishmentType.SoftBan:
            {
                _ = e.DeleteAsync();
                _ = member.BanAsync(7, this.Bot.Guilds[guild.Id].PhishingDetection.CustomPunishmentReason.Replace("%R", this.tKey.DetectedMaliciousHost.Get(this.Bot.Guilds[guild.Id]).Build(new TVar("Host", url))));
                await Task.Delay(1000);
                _ = member.UnbanAsync();
                break;
            }
            case PhishingPunishmentType.Ban:
            {
                _ = e.DeleteAsync();
                _ = member.BanAsync(7, this.Bot.Guilds[guild.Id].PhishingDetection.CustomPunishmentReason.Replace("%R", this.tKey.DetectedMaliciousHost.Get(this.Bot.Guilds[guild.Id]).Build(new TVar("Host", url))));
                break;
            }
        }
    }

    private Dictionary<string, DateTime> recentlyResolvedUrls = new();
}
