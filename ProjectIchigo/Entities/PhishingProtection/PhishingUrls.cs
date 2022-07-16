namespace ProjectIchigo;

public class PhishingUrls
{
    internal Dictionary<string, UrlInfo> List = new();

    public class UrlInfo
    {
        public string Url { get; set; } = "";
        public List<string> Origin { get; set; } = new();
        public ulong Submitter { get; set; } = 0;
    }
}