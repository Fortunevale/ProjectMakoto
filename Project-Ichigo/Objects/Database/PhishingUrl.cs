namespace Project_Ichigo.Objects.Database;

public class PhishingUrlInfo
{
    public string Url { get; set; } = "";
    public string Origin { get; set; } = "";
    public ulong Submitter { get; set; }
}
