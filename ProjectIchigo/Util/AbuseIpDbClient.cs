namespace ProjectIchigo.Util;

internal class AbuseIpDbClient
{
    public static AbuseIpDbClient Initialize(Bot bot)
    {
        AbuseIpDbClient abuseIpDbClient = new();

        abuseIpDbClient._bot = bot;

        _ = abuseIpDbClient.QueueHandler();
        return abuseIpDbClient;
    }

    private Bot _bot { get; set; }

    private readonly Dictionary<string, RequestItem> Queue = new();

    private Dictionary<string, Tuple<AbuseIpDbQuery, DateTime>> Cache = new();

    private int RequestsRemaining = 1;

    private async Task QueueHandler()
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Add("Key", _bot.status.LoadedConfig.Secrets.AbuseIpDbToken);
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        while (true)
        {
            if (Queue.Count == 0 || !Queue.Any(x => !x.Value.Resolved && !x.Value.Failed))
            {
                await Task.Delay(100);
                continue;
            }

            var b = Queue.First(x => !x.Value.Resolved && !x.Value.Failed);

            try
            {
                var response = await client.PostAsync(b.Value.Url, null);

                Queue[b.Key].StatusCode = response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new Exceptions.NotFoundException();

                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        throw new Exceptions.InternalServerErrorException();

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                        throw new Exceptions.ForbiddenException();

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        throw new Exceptions.TooManyRequestsException();

                    throw new Exception($"Unhandled, unsuccessful request: {response.StatusCode}");
                }

                RequestsRemaining = response.Headers.First(x => x.Key == "X-RateLimit-Remaining").Value.First().ToInt32();

                Queue[b.Key].Response = await response.Content.ReadAsStringAsync();
                Queue[b.Key].Resolved = true;
            }
            catch (Exception ex)
            {
                Queue[b.Key].Failed = true;
                Queue[b.Key].Exception = ex;
            }
            finally
            {
                await Task.Delay(1000);
            }
        }
    }

    private async Task<string> MakeRequest(string url)
    {
        string key = Guid.NewGuid().ToString();
        Queue.Add(key, new RequestItem { Url = url });

        while (Queue.ContainsKey(key) && !Queue[key].Resolved && !Queue[key].Failed)
            await Task.Delay(100);

        if (!Queue.ContainsKey(key))
            throw new Exception("The request has been removed from the queue prematurely.");

        var response = Queue[key];
        Queue.Remove(key);

        if (response.Resolved)
            return response.Response;

        if (response.Failed)
            throw response.Exception;

        throw new Exception("This exception should be impossible to get.");
    }

    public async Task<AbuseIpDbQuery> QueryIp(string Ip)
    {
        string query;

        using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "ipAddress", Ip },
                    { "maxAgeInDays", "90" },
                    { "verbose", "" },
                }))
        {
            query = await content.ReadAsStringAsync();
        }

        if (Cache.ContainsKey(Ip) && Cache[Ip].Item2.AddHours(4).GetTotalSecondsUntil() > 0)
            return Cache[Ip].Item1;
        else if (Cache.ContainsKey(Ip))
            Cache.Remove(Ip);

        var rawResponse = await MakeRequest($"https://api.abuseipdb.com/api/v2/check?{query}");
        var parsedResponse = JsonConvert.DeserializeObject<AbuseIpDbQuery>(rawResponse);

        Cache.Add(Ip, new Tuple<AbuseIpDbQuery, DateTime>(parsedResponse, DateTime.UtcNow));
        return parsedResponse;
    }
}
