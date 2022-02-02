namespace Project_Ichigo.PhishingProtection;

public class PhishingUrlUpdater
{

    public async Task UpdatePhishingUrlDatabase(PhishingUrls phishingUrls)
    {
        var urls = await GetUrls();

        bool DatabaseUpdated = false;

        foreach (var b in urls)
        {
            if (!phishingUrls.List.ContainsKey(b.Url))
            {
                DatabaseUpdated = true;
                phishingUrls.List.Add(b.Url, b);

                LogDebug($"Added '{b.Url}' to the phishing url database");
                continue;
            }

            if (phishingUrls.List.ContainsKey(b.Url))
            {
                if (phishingUrls.List[b.Url].Origin.Count != b.Origin.Count)
                {
                    DatabaseUpdated = true;
                    phishingUrls.List[ b.Url ].Origin = b.Origin;
                    phishingUrls.List[ b.Url ].Submitter = b.Submitter;

                    LogDebug($"Updated '{b.Url}' in the phishing url database");
                    continue;
                }
            }
        }

        List<string> dropUrls = new();

        if (phishingUrls.List.Any(x => x.Value.Origin.Count != 0 && !urls.Any(y => y.Url == x.Value.Url)))
            foreach (var b in phishingUrls.List.Where(x => x.Value.Origin.Count != 0 && !urls.Any(y => y.Url == x.Value.Url)).ToList())
            {
                DatabaseUpdated = true;
                phishingUrls.List.Remove(b.Key);
                dropUrls.Add(b.Key);

                LogDebug($"Removed '{b.Value.Url}' from the phishing url database");
            }

        GC.Collect();

        if (!DatabaseUpdated)
        {
            LogDebug($"Nothing has been updated");
            return;
        }

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
            LogDebug($"Generating DatabaseInserts..");
            UpdateRunning = true;
            List<PhishingUrlInfo> DatabaseInserts = phishingUrls.List.Select(x => new PhishingUrlInfo
            {
                Url = x.Value.Url,
                Origin = JsonConvert.SerializeObject(x.Value.Origin),
                Submitter = x.Value.Submitter
            }).OrderBy(x => x.Url).ToList();

            if (Bot.databaseConnection == null)
            {
                throw new Exception($"Exception occured while trying to update phishing urls saved in database: Database connection not present");
            }

            Stopwatch sw = Stopwatch.StartNew();

            var cmd = Bot.databaseConnection.CreateCommand();
            cmd.CommandText = @$"INSERT INTO scam_urls ( url, origin, submitter ) VALUES ";

            for (int i = 0; i < DatabaseInserts.Count; i++)
            {
                cmd.CommandText += @$"( @url{i}, @origin{i}, @submitter{i} ), ";

                cmd.Parameters.AddWithValue($"url{i}", DatabaseInserts[ i ].Url);
                cmd.Parameters.AddWithValue($"origin{i}", DatabaseInserts[ i ].Origin);
                cmd.Parameters.AddWithValue($"submitter{i}", DatabaseInserts[ i ].Submitter);
            }

            cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
            cmd.CommandText += " ON DUPLICATE KEY UPDATE origin=values(origin)";

            cmd.Connection = Bot.databaseConnection;

            LogDebug($"Inserting {DatabaseInserts.Count} rows into table 'scam_urls'..");
            await cmd.ExecuteNonQueryAsync();

            sw.Stop();
            LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'scam_urls'. ({sw.ElapsedMilliseconds}ms)");
            UpdateRunning = false;
            DatabaseInserts.Clear();
            DatabaseInserts = null;

            if (dropUrls.Count != 0)
                foreach (var b in dropUrls)
                {
                    sw.Restart();
                    LogDebug($"Dropping '{b}' from table 'scam_urls'..");

                    cmd = Bot.databaseConnection.CreateCommand();
                    cmd.CommandText = $"DELETE FROM scam_urls WHERE url='{b}'";
                    cmd.Connection = Bot.databaseConnection;
                    await cmd.ExecuteNonQueryAsync();

                    LogDebug($"Dropped '{b}' from table 'scam_urls'. ({sw.ElapsedMilliseconds}ms)");
                    sw.Stop();
                }

            sw = null;
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
        Stopwatch sw = Stopwatch.StartNew();

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

        LogDebug($"Downloaded all phishing urls. ({sw.ElapsedMilliseconds}ms)");

        sw.Restart();
        try
        {
            var urls = await DownloadList("https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt");
            WhitelistedDomains.AddRange(urls);
        }
        catch (Exception ex) { LogError($"An exception occured while trying to download URLs from 'https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt': {ex}"); }

        LogDebug($"Downloaded whitelist for phishing urls. ({sw.ElapsedMilliseconds}ms)");

        sw.Restart();
        try
        {
            if (WhitelistedDomains is null || WhitelistedDomains.Count == 0)
                throw new Exception($"An exception occured while trying to remove whitelisted URLs from blacklist: WhitelistedDomains is empty or null");

            foreach (var b in WhitelistedDomains)
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.Remove(b);
        }
        catch (Exception ex) { LogError($"Failed to remove whitelisted domains from blacklist: {ex}"); }

        LogDebug($"Removed whitelisted urls from phishing urls. ({sw.ElapsedMilliseconds}ms)");
        sw.Stop();

        return SanitizedMatches.Select(x => new PhishingUrls.UrlInfo
        {
            Url = x.Key,
            Origin = x.Value,
            Submitter = 0
        }).ToList();
    }

    private async Task<List<string>> DownloadList(string url)
    {
        LogDebug($"Downloading URLs as List from '{url}'..");

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