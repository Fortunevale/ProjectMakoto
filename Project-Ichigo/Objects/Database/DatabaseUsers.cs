namespace Project_Ichigo.Objects.Database;

internal class DatabaseUsers
{
    public ulong userid { get; set; }
    public ulong scoresaber_id { get; set; }
    public ulong afk_since { get; set; }
    public string afk_reason { get; set; }
    public long afk_pingamount { get; set; }
    public string afk_pings { get; set; }
    public bool submission_accepted_tos { get; set; }
    public bool experience_directmessageoptout { get; set; }
    public string submission_accepted_submissions { get; set; }
    public DateTime submission_last_datetime { get; set; }
}
