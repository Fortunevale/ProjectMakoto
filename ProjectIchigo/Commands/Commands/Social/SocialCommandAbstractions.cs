namespace ProjectIchigo.Commands;
internal class SocialCommandAbstractions
{
    internal static async Task<string> GetGif(Bot bot, string action)
    {
        KawaiiResponse request = JsonConvert.DeserializeObject<KawaiiResponse>(await new HttpClient().GetStringAsync($"https://kawaii.red/api/gif/{action}/token={bot.status.LoadedConfig.Secrets.KawaiiRedToken}/"));
        return request.response;
    }
}
