namespace Project_Ichigo.Objects.Database;

public class DatabasePhishingUrlInfo
{
    public string url { get; set; } = "";
    public string origin { get; set; } = "";
    public ulong submitter { get; set; }
}
