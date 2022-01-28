namespace Project_Ichigo.Objects.Database;

public class PhishingUrlInfo
{
    public int ind { get; set; } = 0;
    public string Url { get; set; } = "";
    public string Origin { get; set; } = "";
    public ulong Submitter { get; set; }
}
