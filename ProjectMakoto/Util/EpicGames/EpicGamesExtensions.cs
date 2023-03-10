namespace ProjectMakoto.Util.EpicGames;

internal static class EpicGamesExtensions
{
    public static HttpClient InitializeClientWithDefaultHeaders(this HttpClient client, EpicGamesClient epic)
    {
        client.DefaultRequestHeaders.Add("User-Agent", $"ProjectMakoto/{epic._bot.status.LoadedConfig.DontModify.LastStartedVersion}");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        return client;
    }
}
