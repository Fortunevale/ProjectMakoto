namespace ProjectIchigo.Util;

internal class GoogleTranslateClient
{
    public static GoogleTranslateClient Initialize()
    {
        GoogleTranslateClient translationClient = new();
        _ = translationClient.QueueHandler();
        return translationClient;
    }

    internal DateTime LastRequest = DateTime.MinValue;
    internal readonly Dictionary<string, RequestItem> Queue = new();

    private async Task QueueHandler()
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36");

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
                        throw new Exceptions.NotFoundException("");

                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        throw new Exceptions.InternalServerErrorException("");

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                        throw new Exceptions.ForbiddenException("");

                    throw new Exception($"Unsuccessful request: {response.StatusCode}");
                }


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
                LastRequest = DateTime.UtcNow;
                await Task.Delay(10000);
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

    public async Task<Tuple<string, string>> Translate_a(string sl, string tl, string q)
    {
        string query;

        using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "sl", sl },
                    { "tl", tl },
                    { "q", q },
                }))
        {
            query = await content.ReadAsStringAsync();
        }

        var translateResponse = await MakeRequest($"https://translate.google.com/translate_a/single?client=gtx&{query}&dt=t&ie=UTF-8&oe=UTF-8");

        var parsedResponse = JsonConvert.DeserializeObject<object[]>(translateResponse);
        var parsedTextStep1 = JsonConvert.DeserializeObject<object[]>(parsedResponse[0].ToString());
        string translatedText = string.Join(" ", parsedTextStep1.Select(x => JsonConvert.DeserializeObject<object[]>(x.ToString())[0].ToString()));

        string translationSource = "";

        if (sl == "auto")
        {
            var parsedLanguageStep1 = JsonConvert.DeserializeObject<object[]>(parsedResponse[8].ToString());
            var parsedLanguageStep2 = JsonConvert.DeserializeObject<object[]>(parsedLanguageStep1[0].ToString());
            translationSource = parsedLanguageStep2[0].ToString(); 
        }

        return new Tuple<string, string>(translatedText, translationSource);
    }
}
