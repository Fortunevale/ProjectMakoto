// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Net.Http.Headers;

namespace ProjectMakoto.Util;

internal sealed class PhishingUrlHandler : RequiresBotReference
{
    public PhishingUrlHandler(Bot bot) : base(bot)
    {
    }

    public async Task UpdatePhishingUrlDatabase()
    {
        try
        {
            _ = new Func<Task>(async () =>
            {
                _ = this.UpdatePhishingUrlDatabase();
            }).CreateScheduledTask(DateTime.UtcNow.AddMinutes(30));

            var urls = await this.GetUrls();
            var listFailed = false;

            foreach (var (Url, Origins, ListFailed) in urls.GroupBy(x => x.Url).First())
            {
                if (ListFailed)
                    listFailed = true;

                if (!this.Bot.PhishingHosts.ContainsKey(Url))
                {
                    this.Bot.PhishingHosts.Add(Url, new PhishingUrlEntry(this.Bot, Url)
                    {
                        Url = Url,
                        Origin = Origins
                    });
                    continue;
                }

                if (this.Bot.PhishingHosts.ContainsKey(Url))
                {
                    if (this.Bot.PhishingHosts[Url].Origin?.Length != Origins.Length)
                    {
                        this.Bot.PhishingHosts[Url].Origin = Origins;
                        continue;
                    }
                }
            }

            if (!listFailed)
                foreach (var b in this.Bot.PhishingHosts)
                {
                    if (b.Value.Submitter != 0)
                        continue;

                    if (!urls.Any(x => x.Url == b.Key))
                        _ = this.Bot.PhishingHosts.Remove(b.Key);
                }

            urls.Clear();
            GC.Collect();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update Phishing Urls", ex);
        }
    }

    private async Task<List<(string Url, string[] Origins, bool ListFailed)>> GetUrls()
    {
        List<string> WhitelistedDomains = new();
        Dictionary<string, List<string>> SanitizedMatches = new();
        var listFailed = false;

        foreach (var url in new string[]
        {
            "https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json",
            "https://raw.githubusercontent.com/nikolaischunk/discord-phishing-links/main/suspicious-list.json",
            "https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt",
            "https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt",
            "https://fortunevale.de/discord-scam-urls.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt",
            "https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt"
        })
        {
            try
            {
                var list = await this.DownloadList(url);

                foreach (var b in list)
                {
                    if (SanitizedMatches.ContainsKey(b))
                        SanitizedMatches.First(x => x.Key == b).Value.Add(url);
                    else
                        SanitizedMatches.Add(b, new List<string> { url });
                }
            }
            catch (Exception ex)
            {
                listFailed = true;
                _logger.LogError("An exception occurred while trying to download URLs from '{url}'", ex, url);
            }
        }

        try
        {
            var urls = await this.DownloadList("https://fortunevale.de/discord-scam-urls-whitelist.txt");
            WhitelistedDomains.AddRange(urls);
        }
        catch (Exception ex) { throw new Exception($"An exception occurred while trying to download URLs from 'https://fortunevale.de/discord-scam-urls-whitelist.txt'", ex); }

        try
        {
            if (WhitelistedDomains is null || WhitelistedDomains.Count == 0)
                throw new Exception($"An exception occurred while trying to remove white listed URLs from blacklist: WhitelistedDomains is empty or null");

            foreach (var b in WhitelistedDomains)
                _ = SanitizedMatches.Remove(b);
        }
        catch (Exception ex) { throw new Exception($"Failed to remove whitelisted domains from blacklist", ex); }

        return SanitizedMatches.Select(x => (x.Key, x.Value.ToArray(), listFailed)).ToList();
    }

    private async Task<List<string>> DownloadList(string url)
    {
        HttpClient client = new();

        var productValue = new ProductInfoHeaderValue("ProjectMakoto", this.Bot.status.RunningVersion);
        var commentValue = new ProductInfoHeaderValue("(+https://fortunevale.de)");

        client.DefaultRequestHeaders.UserAgent.Add(productValue);
        client.DefaultRequestHeaders.UserAgent.Add(commentValue);

        var urls = await client.GetStringAsync(url);

        return urls.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLower()
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(",", "")
                .Replace("127.0.0.1", "").Trim())
                .ToList()
            .Where(x => !x.StartsWith("#") && !x.StartsWith("!") && x.Contains('.')).ToList();
    }
}