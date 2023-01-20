namespace ProjectIchigo.Commands;
internal class SocialCommandAbstractions
{
    static HttpClient httpClient = null;

    internal static async Task<Tuple<string, string>> GetGif(Bot bot, string action)
    {
        if (httpClient is null)
        {
            httpClient = new HttpClient(new SocketsHttpHandler() { PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
            httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        try
        {
            KawaiiResponse request = JsonConvert.DeserializeObject<KawaiiResponse>(await httpClient.GetStringAsync($"https://kawaii.red/api/gif/{action}/token={bot.status.LoadedConfig.Secrets.KawaiiRedToken}/"));
            return new Tuple<string, string>("kawaii.red", request.response);
        }
        catch (Exception ex)
        {
            _logger.LogWarn("Failed to fetch gif from kawaii.red", ex);

            try
            {
                if (!new string[] { "cuddle", "slap", "pat", "hug", "kiss" }.Contains(action))
                    throw new NotSupportedException("Unsupported gif type");

                NekosLifeRequest request = JsonConvert.DeserializeObject<NekosLifeRequest>(await httpClient.GetStringAsync($"https://nekos.life/api/v2/img/{action}"));
                return new Tuple<string, string>("nekos.life", request.url);
            }
            catch (Exception ex1)
            {
                _logger.LogError("Failed to fetch gif from kawaii.red & nekos.life", ex1);
                return new Tuple<string, string>("GIF Service currently unavailable", "");
            }
        }
    }
}
