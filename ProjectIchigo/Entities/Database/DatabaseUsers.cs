namespace ProjectIchigo.Entities.Database;

internal class DatabaseUsers
{
    public ulong userid { get; set; }
    public ulong scoresaber_id { get; set; }
    public long afk_since { get; set; }
    public string afk_reason { get; set; }
    public string playlists { get; set; }
    public long afk_pingamount { get; set; }
    public string afk_pings { get; set; }
    public int submission_accepted_tos { get; set; }
    public bool experience_directmessageoptout { get; set; }
    public string submission_accepted_submissions { get; set; }
    public long submission_last_datetime { get; set; }
}
