namespace ProjectIchigo.Entities;

public class InviteTrackerMember
{
    private ulong _UserId { get; set; } = 0;
    public ulong UserId { get => _UserId; set { _UserId = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private string _Code { get; set; } = "";
    public string Code { get => _Code; set { _Code = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}
