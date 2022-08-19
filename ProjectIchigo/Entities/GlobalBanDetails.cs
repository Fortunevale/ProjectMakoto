namespace ProjectIchigo.Entities;

internal class GlobalBanDetails
{
    private string _Reason { get; set; }
    public string Reason { get => _Reason; set { _Reason = value; _ = Bot.DatabaseClient.FullSyncDatabase(); } }


    private ulong _Moderator { get; set; }
    public ulong Moderator { get => _Moderator; set { _Moderator = value; _ = Bot.DatabaseClient.FullSyncDatabase(); } }


    private DateTime _Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime Timestamp { get => _Timestamp; set { _Timestamp = value; _ = Bot.DatabaseClient.FullSyncDatabase(); } }
}
