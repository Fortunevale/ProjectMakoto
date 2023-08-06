// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.ScoreSaber;
using ProjectMakoto.Enums.ScoreSaber;

namespace ProjectMakoto.Util;

public class ScoreSaberClient : RequiresBotReference
{
    internal ScoreSaberClient(Bot bot) : base(bot)
    {
        this.QueueHandler();
    }

    ~ScoreSaberClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    readonly Dictionary<string, WebRequestItem> Queue = new();

    private void QueueHandler()
    {
        _ = Task.Run(async () =>
        {
            HttpClient client = new();

            while (!this._disposed)
            {
                if (this.Queue.Count == 0 || !this.Queue.Any(x => !x.Value.Resolved && !x.Value.Failed))
                {
                    await Task.Delay(100);
                    continue;
                }

                var b = this.Queue.First(x => !x.Value.Resolved && !x.Value.Failed);

                try
                {
                    _logger.LogTrace("Sending Request to '{Url}'..", b.Value.Url);
                    var response = await client.GetAsync(b.Value.Url);

                    this.Queue[b.Key].StatusCode = response.StatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                            throw new NotFoundException("");

                        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                            throw new UnprocessableEntityException("");

                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            throw new InternalServerErrorException("");

                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            throw new ForbiddenException("");

                        throw new Exception($"Unsuccessful request: {response.StatusCode}");
                    }


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
                    await Task.Delay(100);
                }
            }
        }).Add(this.Bot).IsVital();
    }

    private async Task<string> MakeRequest(string url)
    {
        var key = Guid.NewGuid().ToString();
        this.Queue.Add(key, new WebRequestItem { Url = url });

        while (this.Queue.ContainsKey(key) && !this.Queue[key].Resolved && !this.Queue[key].Failed)
            await Task.Delay(100);

        if (!this.Queue.ContainsKey(key))
            throw new Exception("The request has been removed from the queue prematurely.");

        var response = this.Queue[key];
        _ = this.Queue.Remove(key);

        if (response.Resolved)
            return response.Response;

        if (response.Failed)
            throw response.Exception;

        throw new Exception("This exception should be impossible to get.");
    }

    /// <summary>
    /// Gets a player by the given id
    /// </summary>
    /// <param name="id">The id to request.</param>
    /// <returns>The requested player.</returns>
    public async Task<PlayerInfo> GetPlayerById(string id)
    {
        var response = await this.MakeRequest($"https://scoresaber.com/api/player/{id}/full");
        var request = JsonConvert.DeserializeObject<PlayerInfo>(response);
        return request;
    }

    /// <summary>
    /// Gets a scoreboard by the given id.
    /// </summary>
    /// <param name="id">The id to request.</param>
    /// <returns>The requested scoreboard.</returns>
    public async Task<Leaderboard> GetScoreboardById(string id)
    {
        var response = await this.MakeRequest($"https://scoresaber.com/api/ranking/request/by-id/{id}");
        var request = JsonConvert.DeserializeObject<Leaderboard>(response);

        return request;
    }

    /// <summary>
    /// Gets a scoreboard by the map hash.
    /// </summary>
    /// <param name="hash">The hash to request.</param>
    /// <returns>The requested scoreboard.</returns>
    public async Task<Leaderboard> GetScoreboardByHash(string hash)
    {
        var response = await this.MakeRequest($"https://scoresaber.com/api/leaderboard/by-hash/{hash}");
        var request = JsonConvert.DeserializeObject<Leaderboard>(response);

        return request;
    }

    /// <summary>
    /// Gets a scoreboard by it's id.
    /// </summary>
    /// <param name="id">The id to request.</param>
    /// <param name="page">The page to request.</param>
    /// <param name="country">Only gets scores from this country.</param>
    /// <returns>The leaderboard and request metadata.</returns>
    public async Task<LeaderboardScores> GetScoreboardScoresById(string id, uint page = 1, string? country = null)
    {
        if (page < 1)
            page = 1;

        Dictionary<string, string> parameters = new()
        {
            { "page", page.ToString() }
        };

        if (country is not null)
            parameters.Add("countries", country.ToLower());

        var query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();

        var response = await this.MakeRequest($"https://scoresaber.com/api/leaderboard/by-id/{id}/scores?{query}");
        var parsedResponse = JsonConvert.DeserializeObject<LeaderboardScores>(response);

        return parsedResponse;
    }

    /// <summary>
    /// Gets scoreboard by the map hash and difficulty.
    /// </summary>
    /// <param name="hash">The hash to request.</param>
    /// <param name="difficulty">The difficulty to request.</param>
    /// <param name="page">The page to request.</param>
    /// <param name="country">Only gets scores from this country.</param>
    /// <returns>The leaderboard and request metadata.</returns>
    public async Task<LeaderboardScores> GetScoreboardScoresByHash(string hash, Difficulty difficulty, uint page = 1, string? country = null)
    {
        if (page < 1)
            page = 1;

        Dictionary<string, string> parameters = new()
        {
            { "difficulty", ((int)difficulty).ToString() },
            { "page", page.ToString() },
        };

        if (country is not null)
            parameters.Add("countries", country.ToLower());

        var query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();

        var response = await this.MakeRequest($"https://scoresaber.com/api/leaderboard/by-hash/{hash}/scores?{query}");
        var request = JsonConvert.DeserializeObject<LeaderboardScores>(response);

        return request;
    }

    /// <summary>
    /// Gets a player's scores by the player's id.
    /// </summary>
    /// <param name="id">The id to request.</param>
    /// <param name="sortType">The type to sort by.</param>
    /// <param name="limit">How many scores should be shown.</param>
    /// <param name="page">Which page to request.</param>
    /// <returns>The player's scores.</returns>
    public async Task<PlayerScores> GetScoresById(string id, ScoreType sortType, uint limit = 10, uint page = 1)
    {
        if (page < 1)
            page = 1;

        Dictionary<string, string> parameters = new()
        {
            { "limit", limit.ToString() },
            { "sort", (sortType == ScoreType.Top ? "top" : "recent") },
            { "page", page.ToString() },
            { "withMetadata", "true" }
        };

        var query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();

        var response = await this.MakeRequest($"https://scoresaber.com/api/player/{id}/scores?{query}");
        var request = JsonConvert.DeserializeObject<PlayerScores>(response);

        return request;
    }

    /// <summary>
    /// Searches for players matching a string.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="page">The page of the search to request.</param>
    /// <param name="country">Only show players from this country.</param>
    /// <returns>A list of users of the specified filter.</returns>
    public async Task<PlayerSearch> SearchPlayer(string name, int page = 1, string? country = null)
    {
        if (page < 1)
            page = 1;

        Dictionary<string, string> parameters = new()
        {
            { "search", name },
            { "page", page.ToString() }
        };

        if (country is not null)
            parameters.Add("countries", country.ToLower());

        var query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();

        var response = await this.MakeRequest($"https://scoresaber.com/api/players?{query}");
        var request = JsonConvert.DeserializeObject<PlayerSearch>(response);

        return request;
    }
}
