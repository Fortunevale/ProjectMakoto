namespace Project_Ichigo.PhishingProtection;
internal class SubmittedUrls
{
    public Dictionary<ulong, UrlInfo> List = new();

    public class UrlInfo
    {
        public string Url { get; set; }
        public ulong Submitter { get; set; }
        public ulong GuildOrigin { get; set; }
    }
}
