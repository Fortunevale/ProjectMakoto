namespace Project_Ichigo.PhishingProtection;

public class PhishingUrlUpdater
{

    public async Task UpdatePhishingUrlDatabase(PhishingUrls phishingUrls)
    {
        var urls = await GetUrls();

        bool DatabaseUpdated = false;

        foreach (var b in urls)
        {
            if (!phishingUrls.List.Any(x => x.Url == b.Url))
            {
                DatabaseUpdated = true;
                phishingUrls.List.Add(b);

                LogDebug($"Added '{b.Url}' ('{(b.Origin.Count != 0 ? String.Join(", ", b.Origin) : $"{b.Submitter}")}') to the phishing url database");
            }

            if (phishingUrls.List.Any(x => x.Url == b.Url && x.Origin.Count != b.Origin.Count || x.Submitter != x.Submitter))
            {
                DatabaseUpdated = true;
                phishingUrls.List.Remove(phishingUrls.List.First(x => x.Url == b.Url));
                phishingUrls.List.Add(b);

                LogDebug($"Updated '{b.Url}' ('{(b.Origin.Count != 0 ? String.Join(", ", b.Origin) : $"{b.Submitter}")}') in the phishing url database");
            }
        }

        if (phishingUrls.List.Any(x => x.Origin.Count != 0 && !urls.Any(y => y.Url == x.Url)))
            foreach (var b in phishingUrls.List.ToList())
            {
                DatabaseUpdated = true;
                phishingUrls.List.Remove(phishingUrls.List.First(x => x.Url == b.Url));

                LogDebug($"Removed '{b.Url}' ('{(b.Origin.Count != 0 ? String.Join(", ", b.Origin) : $"{b.Submitter}")}') from the phishing url database");
            }

        if (!DatabaseUpdated)
        {
            LogDebug($"Nothing has been updated");
            return;
        }

        await UpdateDatabase(phishingUrls);
    }

    private bool UpdateRunning = false;

    public async Task UpdateDatabase(PhishingUrls phishingUrls)
    {
        if (UpdateRunning)
        {
            LogWarn($"A database update is already running, cancelling");
            return;
        }

        try
        {
            UpdateRunning = true;
            List<PhishingUrls.UrlInfoDatabase> DatabaseInserts = phishingUrls.List.Select(x => new PhishingUrls.UrlInfoDatabase
            {
                Url = x.Url,
                Origin = JsonConvert.SerializeObject(x.Origin),
                Submitter = x.Submitter
            }).OrderBy(x => x.Url).ToList();

            if (Bot.databaseConnection == null)
            {
                throw new Exception($"Exception occured while trying to update phishing urls saved in database: Database connection not present");
            }

            Stopwatch sw = Stopwatch.StartNew();

            var clearcmd = Bot.databaseConnection.CreateCommand();
            clearcmd.CommandText = "TRUNCATE TABLE scam_urls";
            clearcmd.Connection = Bot.databaseConnection;
            await clearcmd.ExecuteNonQueryAsync();

            sw.Stop();
            LogDebug($"Cleared table 'scam_urls'. ({sw.ElapsedMilliseconds}ms)");

            sw.Restart();

            var cmd = Bot.databaseConnection.CreateCommand();
            cmd.CommandText = @$"INSERT INTO scam_urls ( ind, url, origin, submitter ) VALUES ";

            for (int i = 0; i < DatabaseInserts.Count; i++)
            {
                cmd.CommandText += @$"( @ind{i}, @url{i}, @origin{i}, @submitter{i} ), ";

                cmd.Parameters.AddWithValue($"ind{i}", i);
                cmd.Parameters.AddWithValue($"url{i}", DatabaseInserts[ i ].Url);
                cmd.Parameters.AddWithValue($"origin{i}", DatabaseInserts[ i ].Origin);
                cmd.Parameters.AddWithValue($"submitter{i}", DatabaseInserts[ i ].Submitter);
            }

            cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);

            cmd.Connection = Bot.databaseConnection;

            LogDebug($"Inserting {DatabaseInserts.Count} rows into table 'scam_urls'..");
            await cmd.ExecuteNonQueryAsync();

            sw.Stop();
            LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'scam_urls'. ({sw.ElapsedMilliseconds}ms)");
            UpdateRunning = false;
        }
        catch (Exception)
        {
            UpdateRunning = false;
            throw;
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

        HttpClient client = new HttpClient();
        var urls = await client.GetStringAsync(url);

        return urls.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLower()
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(",", "")
                .Replace("127.0.0.1", "").Trim())
                .ToList()
            .Where(x => !x.StartsWith("#") && !x.StartsWith("!") && x.Contains(".")).ToList();
    }
}