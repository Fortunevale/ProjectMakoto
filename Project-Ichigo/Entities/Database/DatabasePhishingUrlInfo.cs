namespace Project_Ichigo.Entities.Database;

public class DatabasePhishingUrlInfo
{
    public string url { get; set; } = "";
    public string origin { get; set; } = "";
    public ulong submitter { get; set; }
}
