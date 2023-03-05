namespace ProjectMakoto.Util;

internal class PhishingUrlUpdater
{
    internal PhishingUrlUpdater(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    public async Task UpdatePhishingUrlDatabase()
    {
        new Task(new Action(async () =>
        {
            _ = UpdatePhishingUrlDatabase();
        })).CreateScheduleTask(DateTime.UtcNow.AddMinutes(30), $"phishing-update");

        var urls = await GetUrls();

        bool DatabaseUpdated = false;

        foreach (var b in urls)
        {
            if (!_bot.phishingUrls.ContainsKey(b.Url))
            {
                DatabaseUpdated = true;
                _bot.phishingUrls.Add(b.Url, b);
                continue;
            }

            if (_bot.phishingUrls.ContainsKey(b.Url))
            {
                if (_bot.phishingUrls[b.Url].Origin.Count != b.Origin.Count)
                {
                    DatabaseUpdated = true;
                    _bot.phishingUrls[ b.Url ].Origin = b.Origin;
                    _bot.phishingUrls[ b.Url ].Submitter = b.Submitter;
                    continue;
                }
            }
        }

        List<string> dropUrls = new();

        if (_bot.phishingUrls.Any(x => x.Value.Origin.Count != 0 && x.Value.Submitter != 0 && !urls.Any(y => y.Url == x.Value.Url)))
            foreach (var b in _bot.phishingUrls.Where(x => x.Value.Origin.Count != 0 && x.Value.Submitter != 0 && !urls.Any(y => y.Url == x.Value.Url)).ToList())
            {
                DatabaseUpdated = true;
                _bot.phishingUrls.Remove(b.Key);
                dropUrls.Add(b.Key);
            }

        GC.Collect();

        if (!DatabaseUpdated)
            return;

        try
        {
            await UpdateDatabase(dropUrls);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update database", ex);
        }
    }

    private bool UpdateRunning = false;

    public async Task UpdateDatabase(List<string> dropUrls)
    {
        if (UpdateRunning)
        {
            _logger.LogWarn("A database update is already running, cancelling");
            return;
        }

        try
        {
            UpdateRunning = true;

            await _bot.databaseClient.FullSyncDatabase();

            if (dropUrls.Count != 0)
                foreach (var b in dropUrls)
                {
                    await _bot.databaseClient._helper.DeleteRow(_bot.databaseClient.mainDatabaseConnection, "scam_urls", "url", $"{b}");

                    _logger.LogDebug("Dropped '{host}' from table 'scam_urls'.", b);
                }
        }
        catch (Exception)
        {
            GC.Collect();
            UpdateRunning = false;
            throw;
        }
        finally
        {
            await Task.Delay(1000);
            GC.Collect();
        }

        UpdateRunning = false;
    }

    private async Task<List<PhishingUrlEntry>> GetUrls ()
    {
        List<string> WhitelistedDomains = new();
        Dictionary<string, List<string>> SanitizedMatches = new();

        foreach (var url in new string[]
        {
            "https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json",
            "https://raw.githubusercontent.com/nikolaischunk/discord-phishing-links/main/suspicious-list.json",
            "https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt",
            "https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt",
            "https://fortunevale.line.pm/discord-scam-urls.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt",
            "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt",
            "https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt"
        })
        {
            try
            {
                var list = await DownloadList(url);

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
                throw new Exception($"An exception occurred while trying to download URLs from '{url}'", ex);
            }
        }

        try
        {
            var urls = await DownloadList("https://fortunevale.line.pm/discord-scam-urls-whitelist.txt");
            WhitelistedDomains.AddRange(urls);
        }
        catch (Exception ex) { throw new Exception($"An exception occurred while trying to download URLs from 'https://fortunevale.line.pm/discord-scam-urls-whitelist.txt'", ex); }

        try
        {
            if (WhitelistedDomains is null || WhitelistedDomains.Count == 0)
                throw new Exception($"An exception occurred while trying to remove white listed URLs from blacklist: WhitelistedDomains is empty or null");

            foreach (var b in WhitelistedDomains)
                SanitizedMatches.Remove(b);
        }
        catch (Exception ex) { throw new Exception($"Failed to remove whitelisted domains from blacklist", ex); }

        return SanitizedMatches.Select(x => new PhishingUrlEntry
        {
            Url = x.Key,
            Origin = x.Value,
            Submitter = 0
        }).ToList();
    }

    private async Task<List<string>> DownloadList(string url)
    {
        HttpClient client = new();
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