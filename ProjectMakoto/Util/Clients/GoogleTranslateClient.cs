// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public sealed class GoogleTranslateClient : RequiresBotReference
{
    internal GoogleTranslateClient(Bot bot) : base(bot)
    {
        this.QueueHandler();
    }

    ~GoogleTranslateClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    internal DateTime LastRequest = DateTime.MinValue;
    internal readonly Dictionary<string, WebRequestItem> Queue = new();

    private void QueueHandler()
    {
        _ = Task.Run(async () =>
        {
            HttpClient client = new();

            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

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
                    var response = await client.PostAsync(b.Value.Url, null);

                    this.Queue[b.Key].StatusCode = response.StatusCode;

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                            throw new Exceptions.NotFoundException("");

                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            throw new Exceptions.InternalServerErrorException("");

                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            throw new Exceptions.ForbiddenException("");

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
                    this.LastRequest = DateTime.UtcNow;
                    await Task.Delay(10000);
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

        if (!this.Queue.TryGetValue(key, out var value))
            throw new Exception("The request has been removed from the queue prematurely.");

        var response = value;
        _ = this.Queue.Remove(key);

        if (response.Resolved)
            return response.Response;

        if (response.Failed)
            throw response.Exception;

        throw new Exception("This exception should be impossible to get.");
    }

    public async Task<Tuple<string, string>> Translate(string SourceLanguage, string TargetLanguage, string Query)
    {
        string query;

        using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "sl", SourceLanguage },
                    { "tl", TargetLanguage },
                    { "q", Query },
                }))
        {
            query = await content.ReadAsStringAsync();
        }

        var translateResponse = await this.MakeRequest($"https://translate.google.com/translate_a/single?client=gtx&{query}&dt=t&ie=UTF-8&oe=UTF-8");

        var parsedResponse = JsonConvert.DeserializeObject<object[]>(translateResponse);
        var parsedTextStep1 = JsonConvert.DeserializeObject<object[]>(parsedResponse[0].ToString());
        var translatedText = string.Join(" ", parsedTextStep1.Select(x => JsonConvert.DeserializeObject<object[]>(x.ToString())[0].ToString()));

        var translationSource = "";

        if (SourceLanguage == "auto")
        {
            var parsedLanguageStep1 = JsonConvert.DeserializeObject<object[]>(parsedResponse[8].ToString());
            var parsedLanguageStep2 = JsonConvert.DeserializeObject<object[]>(parsedLanguageStep1[0].ToString());
            translationSource = parsedLanguageStep2[0].ToString();
        }

        return new Tuple<string, string>(translatedText, translationSource);
    }
}
