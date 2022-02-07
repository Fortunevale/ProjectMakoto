namespace Project_Ichigo.Objects;

internal class Users
{
    public Dictionary<ulong, Info> List { get; set; } = new();

    internal class Info
    {
        public UrlSubmissions UrlSubmissions { get; set; } = new();
    }

    internal class UrlSubmissions
    {
        public bool AcceptedTOS { get; set; } = false;
        public DateTime LastTime { get; set; } = DateTime.MinValue;
        public List<string> AcceptedSubmissions { get; set; } = new();
    }
}
