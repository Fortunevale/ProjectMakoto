namespace Project_Ichigo.Objects.Database;

public class DatabaseServerSettings
{
    public ulong serverid { get; set; }
    public bool phishing_detect { get; set; }
    public int phishing_type { get; set; }
    public string phishing_reason { get; set; }
    public long phishing_time { get; set; }
}
