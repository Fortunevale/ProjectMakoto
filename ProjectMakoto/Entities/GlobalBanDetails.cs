namespace ProjectMakoto.Entities;

internal class GlobalBanDetails
{
    public string Reason { get; set; }


    public ulong Moderator { get; set; }


    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
