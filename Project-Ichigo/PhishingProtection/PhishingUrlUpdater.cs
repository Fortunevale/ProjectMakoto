namespace Project_Ichigo.PhishingProtection;

public class PhishingUrlUpdater
{
    internal PhishingUrlUpdater(MySqlConnection con, DatabaseHelper helper)
    {
        databaseConnection = con;
        databaseHelper = helper;
    }

    internal MySqlConnection databaseConnection { private get; set; }
    internal DatabaseHelper databaseHelper { private get; set; }

    public async Task UpdatePhishingUrlDatabase(PhishingUrls phishingUrls)
    {
        new Task(new Action(async () =>
        {
            _ = UpdatePhishingUrlDatabase(phishingUrls);
        })).CreateScheduleTask(DateTime.UtcNow.AddMinutes(30), $"phishing-update");

        var urls = await GetUrls();

        bool DatabaseUpdated = false;

        foreach (var b in urls)
        {
            if (!phishingUrls.List.ContainsKey(b.Url))
            {
                DatabaseUpdated = true;
                phishingUrls.List.Add(b.Url, b);
                continue;
            }

            if (phishingUrls.List.ContainsKey(b.Url))
            {
                if (phishingUrls.List[b.Url].Origin.Count != b.Origin.Count)
                {
                    DatabaseUpdated = true;
                    phishingUrls.List[ b.Url ].Origin = b.Origin;
                    phishingUrls.List[ b.Url ].Submitter = b.Submitter;
                    continue;
                }
            }
        }

        List<string> dropUrls = new();

        if (phishingUrls.List.Any(x => x.Value.Origin.Count != 0 && x.Value.Submitter != 0 && !urls.Any(y => y.Url == x.Value.Url)))
            foreach (var b in phishingUrls.List.Where(x => x.Value.Origin.Count != 0 && x.Value.Submitter != 0 && !urls.Any(y => y.Url == x.Value.Url)).ToList())
            {
                DatabaseUpdated = true;
                phishingUrls.List.Remove(b.Key);
                dropUrls.Add(b.Key);
            }

        GC.Collect();

        if (!DatabaseUpdated)
            return;

        try
        {
            await UpdateDatabase(phishingUrls, dropUrls);
        }
        catch (Exception ex)
        {
            LogError($"{ex}");
        }
    }

    private bool UpdateRunning = false;

    public async Task UpdateDatabase(PhishingUrls phishingUrls, List<string> dropUrls)
    {
        if (UpdateRunning)
        {
            LogWarn($"A database update is already running, cancelling");
            return;
        }

        try
        {
            UpdateRunning = true;
            List<DatabasePhishingUrlInfo> DatabaseInserts = phishingUrls.List.Select(x => new DatabasePhishingUrlInfo
            {
                url = x.Value.Url,
                origin = JsonConvert.SerializeObject(x.Value.Origin),
                submitter = x.Value.Submitter
            }).OrderBy(x => x.url).ToList();

            if (databaseConnection == null)
            {
                throw new Exception($"Exception occured while trying to update phishing urls saved in database: Database connection not present");
            }

            var cmd = databaseConnection.CreateCommand();
            cmd.CommandText = databaseHelper.GetSaveCommand("scam_urls", DatabaseColumnLists.scam_urls);

            for (int i = 0; i < DatabaseInserts.Count; i++)
            {
                cmd.CommandText += databaseHelper.GetValueCommand(DatabaseColumnLists.scam_urls, i);

                cmd.Parameters.AddWithValue($"url{i}", DatabaseInserts[ i ].url);
                cmd.Parameters.AddWithValue($"origin{i}", DatabaseInserts[ i ].origin);
                cmd.Parameters.AddWithValue($"submitter{i}", DatabaseInserts[ i ].submitter);
            }

            cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
            cmd.CommandText += databaseHelper.GetOverwriteCommand(DatabaseColumnLists.scam_urls);

            cmd.Connection = databaseConnection;
            await cmd.ExecuteNonQueryAsync();

            LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'scam_urls'.");
            UpdateRunning = false;
            DatabaseInserts.Clear();
            DatabaseInserts = null;

            if (dropUrls.Count != 0)
                foreach (var b in dropUrls)
                {
                    await databaseHelper.DeleteRow("scam_urls", "url", $"{b}");

                    LogDebug($"Dropped '{b}' from table 'scam_urls'.");
                }

            cmd.Dispose();
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
    }

    private async Task<List<PhishingUrls.UrlInfo>> GetUrls ()
    {
        List<string> WhitelistedDomains = new();
        Dictionary<string, List<string>> SanitizedMatches = new();

        foreach (var url in new string[] 
        {
            "https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json",
            "https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt",
            "https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt",
            "https://fortunevale.dd-dns.de/discord-scam-urls.txt",
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
                throw new Exception($"An exception occured while trying to download URLs from '{url}': {ex}");
            }
        }

        try
        {
            var urls = await DownloadList("https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt");
            WhitelistedDomains.AddRange(urls);
        }
        catch (Exception ex) { throw new Exception($"An exception occured while trying to download URLs from 'https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt': {ex}"); }

        try
        {
            if (WhitelistedDomains is null || WhitelistedDomains.Count == 0)
                throw new Exception($"An exception occured while trying to remove whitelisted URLs from blacklist: WhitelistedDomains is empty or null");

            foreach (var b in WhitelistedDomains)
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.Remove(b);
        }
        catch (Exception ex) { throw new Exception($"Failed to remove whitelisted domains from blacklist: {ex}"); }

        return SanitizedMatches.Select(x => new PhishingUrls.UrlInfo
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