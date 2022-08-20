namespace ProjectIchigo.Commands;
internal class SocialCommandAbstractions
{
    internal static async Task<string> GetGif(string action)
    {
        KawaiiResponse request = JsonConvert.DeserializeObject<KawaiiResponse>(await new HttpClient().GetStringAsync($"https://kawaii.red/api/gif/{action}/token={Secrets.Secrets.KawaiiRedToken}/"));
        return request.response;
    }
}
