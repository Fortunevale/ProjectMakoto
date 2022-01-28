namespace Project_Ichigo.Objects;
internal class Users
{
    internal class Info
    {
        public ulong Id { get; set; } = 0;
        public UrlSubmissions UrlSubmissions { get; set; } = new();
    }

    internal class UrlSubmissions
    {
        public bool UrlSubmissionsIsBanned { get; set; } = false;
        public DateTime UrlSubmissionsLastTime { get; set; } = DateTime.MinValue;
        public List<string> UrlSubmissionsSubmittedUrls { get; set; } = new();
    }
}
