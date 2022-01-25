namespace Project_Ichigo.PhishingProtection;

public class PhishingUrlUpdater
{
    public async Task<List<PhishingUrls.UrlInfo>> GetUrls ()
    {
        List<string> WhitelistedDomains = new();
        Dictionary<string, List<string>> SanitizedMatches = new();

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json");
                else
                    SanitizedMatches.Add(b, new List<string>{ "https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json" });
            }
        }
        catch (Exception ex) 
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/nikolaischunk/discord-tokenlogger-link-list/main/domain-list.json': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/DevSpen/links/master/src/links.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/PoorPocketsMcNewHold/SteamScamSites/master/steamscamsite.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://fortunevale.dd-dns.de/discord-scam-urls.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://fortunevale.dd-dns.de/discord-scam-urls.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://fortunevale.dd-dns.de/discord-scam-urls.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://fortunevale.dd-dns.de/discord-scam-urls.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Discord.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Facebook.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/sk-cat/fluffy-blocklist/main/phisising/Steam.txt': {ex}");
        }

        try
        {
            var list = await DownloadList("https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt");

            foreach (var b in list)
            {
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.First(x => x.Key == b).Value.Add("https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt");
                else
                    SanitizedMatches.Add(b, new List<string> { "https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt" });
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"An exception occured while trying to download URLs from 'https://raw.githubusercontent.com/Vytrah/videogame-scam-blocklist/main/list.txt': {ex}");
        }

        try
        {
            var urls = await DownloadList("https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt");
            WhitelistedDomains.AddRange(urls);
        }
        catch (Exception ex) { LogError($"An exception occured while trying to download URLs from 'https://fortunevale.dd-dns.de/discord-scam-urls-whitelist.txt': {ex}"); }

        try
        {
            if (WhitelistedDomains is null || WhitelistedDomains.Count == 0)
                throw new Exception($"An exception occured while trying to remove whitelisted URLs from blacklist: WhitelistedDomains is empty or null");

            foreach (var b in WhitelistedDomains)
                if (SanitizedMatches.ContainsKey(b))
                    SanitizedMatches.Remove(b);
        }
        catch (Exception ex) { LogError($"Failed to remove whitelisted domains from blacklist: {ex}"); }

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
                .Replace("127.0.0.1", "").Trim())
                .ToList()
            .Where(x => !x.StartsWith("#") && x.Contains(".")).ToList();
    }
}