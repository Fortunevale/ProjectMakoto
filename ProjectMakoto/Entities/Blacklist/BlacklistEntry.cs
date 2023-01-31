namespace ProjectMakoto.Entities;

public class BlacklistEntry
{
    public string Reason { get; set; }

    public ulong Moderator { get; set; }


    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
