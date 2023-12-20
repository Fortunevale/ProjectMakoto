// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Concurrent;

namespace ProjectMakoto.Util;

public sealed class AbuseIpDbClient : RequiresBotReference
{
    internal AbuseIpDbClient(Bot bot) : base(bot)
    {
        this.QueueHandler();
    }

    ~AbuseIpDbClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    private readonly ConcurrentDictionary<string, WebRequestItem> Queue = new();

    private Dictionary<string, Tuple<AbuseIpDbQuery, DateTime>> Cache = new();

    private int RequestsRemaining = 1;

    private void QueueHandler()
    {
        _ = Task.Run(async () =>
        {
            HttpClient client = new();

            while (this.Bot.status.LoadedConfig.Secrets.AbuseIpDbToken.IsNullOrWhiteSpace())
            {
                await Task.Delay(5000);
            }

            client.DefaultRequestHeaders.Add("Key", this.Bot.status.LoadedConfig.Secrets.AbuseIpDbToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            while (!this._disposed)
            {
                while (this.RequestsRemaining <= 0)
                {
                    var now = DateTimeOffset.UtcNow;
                    var tomorrow = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero).AddDays(1);

                    _logger.LogWarn("Daily Ratelimit reached for AbuseIPDB. Waiting until {tomorrow}..", tomorrow);
                    var delay = tomorrow - DateTimeOffset.UtcNow;

                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay);

                    _logger.LogInfo("Ratelimit cleared for AbuseIPDB.");
                    this.RequestsRemaining = 1;
                }

                if (this.Queue.IsEmpty || !this.Queue.Any(x => !x.Value.Resolved && !x.Value.Failed))
                {
                    Thread.Sleep(100);
                    continue;
                }

                var b = this.Queue.First(x => !x.Value.Resolved && !x.Value.Failed);

                try
                {
                    var response = await client.GetAsync(b.Value.Url);

                    this.Queue[b.Key].StatusCode = response.StatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                            throw new Exceptions.NotFoundException();

                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            throw new Exceptions.InternalServerErrorException();

                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            throw new Exceptions.ForbiddenException();

                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            this.RequestsRemaining = 0;
                            _logger.LogError("Daily Ratelimit hit for AbuseIPDB.");
                            continue;
                        }

                        throw new Exception($"Unhandled, unsuccessful request: {response.StatusCode}");
                    }

                    this.RequestsRemaining = response.Headers.First(x => x.Key == "X-RateLimit-Remaining").Value.First().ToInt32();
                    _logger.LogDebug("{RequestsRemaining} AbuseIPDB requests remaining.", this.RequestsRemaining);

                    this.Queue[b.Key].Response = await response.Content.ReadAsStringAsync();
                    this.Queue[b.Key].Resolved = true;
                }
                catch (Exception ex)
                {
                    this.Queue[b.Key].Failed = true;
                    this.Queue[b.Key].Exception = ex;
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        }).Add(this.Bot).IsVital();
    }

    private async Task<string> MakeRequest(string url)
    {
        var key = Guid.NewGuid().ToString();
        _ = this.Queue.AddOrUpdate(key, new WebRequestItem { Url = url }, (x1, x2) => { return null; });

        while (this.Queue.ContainsKey(key) && !this.Queue[key].Resolved && !this.Queue[key].Failed)
            await Task.Delay(100);

        if (!this.Queue.TryGetValue(key, out var value))
            throw new Exception("The request has been removed from the queue prematurely.");

        var response = value;
        _ = this.Queue.Remove(key, out _);

        if (response.Resolved)
            return response.Response;
        else if (response.Failed)
            throw response.Exception;
        else
            throw new Exception("This exception should be impossible to get.");
    }

    public async Task<AbuseIpDbQuery> QueryIp(string Ip, bool bypassCache = false)
    {
        while (this.Cache.ContainsKey(Ip) && this.Cache[Ip] is null)
            await Task.Delay(100);

        if (this.Cache.TryGetValue(Ip, out var value) && value.Item2.AddHours(4).GetTotalSecondsUntil() > 0 && !bypassCache)
            return this.Cache[Ip].Item1;
        else
            _ = this.Cache.Remove(Ip);

        this.Cache.Add(Ip, null);

        string query;

        using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "ipAddress", Ip },
                    { "maxAgeInDays", "90" },
                    { "verbose", "true" },
                }))
        {
            query = await content.ReadAsStringAsync();
        }

        var rawResponse = await this.MakeRequest($"https://api.abuseipdb.com/api/v2/check?{query}");
        var parsedResponse = JsonConvert.DeserializeObject<AbuseIpDbQuery>(rawResponse);

        this.Cache[Ip] = new Tuple<AbuseIpDbQuery, DateTime>(parsedResponse, DateTime.UtcNow);
        return parsedResponse;
    }
}
