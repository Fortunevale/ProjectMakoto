namespace Project_Ichigo.Entities.Database;
internal class DatabaseMembers
{
    public ulong userid { get; set; }
    public string saved_nickname { get; set; }
    public string roles { get; set; }
    public ulong first_join { get; set; }
    public ulong last_leave { get; set; }
    public long experience { get; set; }
    public ulong experience_last_message { get; set; }
    public long experience_level { get; set; }
}
