namespace Project_Ichigo.PhishingProtection;

public class PhishingUrls
{
    internal List<UrlInfo> List = new();

    public class UrlInfo
    {
        public string Url { get; set; } = "";
        public List<string> Origin { get; set; } = new();
        public ulong Submitter { get; set; } = 0;
    }
}