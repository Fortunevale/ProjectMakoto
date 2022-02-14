namespace Project_Ichigo.Objects.Database;

public class DatabaseServerSettings
{
    public ulong serverid { get; set; }
    public bool bump_enabled { get; set; }
    public ulong bump_role { get; set; }
    public ulong bump_channel { get; set; }
    public ulong bump_message { get; set; }
    public ulong bump_last_user { get; set; }
    public DateTime bump_last_time { get; set; }
    public DateTime bump_last_reminder { get; set; }
    public bool phishing_detect { get; set; }
    public int phishing_type { get; set; }
    public string phishing_reason { get; set; }
    public long phishing_time { get; set; }
}
