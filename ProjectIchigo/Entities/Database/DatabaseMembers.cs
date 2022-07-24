namespace ProjectIchigo.Entities.Database;
internal class DatabaseMembers
{
    public ulong userid { get; set; }
    public string saved_nickname { get; set; }
    public string roles { get; set; }
    public string invite_code { get; set; }
    public ulong invite_user { get; set; }
    public long first_join { get; set; }
    public long last_leave { get; set; }
    public long experience { get; set; }
    public long experience_last_message { get; set; }
    public long experience_level { get; set; }
}
