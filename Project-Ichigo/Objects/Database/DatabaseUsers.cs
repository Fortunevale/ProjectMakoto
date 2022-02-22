namespace Project_Ichigo.Objects.Database;

internal class DatabaseUsers
{
    public ulong userid { get; set; }
    public ulong afk_since { get; set; }
    public string afk_reason { get; set; }
    public bool submission_accepted_tos { get; set; }
    public string submission_accepted_submissions { get; set; }
    public DateTime submission_last_datetime { get; set; }
}
