namespace Project_Ichigo.Objects;

internal class Users
{
    public Dictionary<ulong, Info> List { get; set; } = new();

    internal class Info
    {
        public UrlSubmissions UrlSubmissions { get; set; } = new();
        public AfkStatus AfkStatus { get; set; } = new();
        public ScoreSaber ScoreSaber { get; set; } = new();
    }
}
