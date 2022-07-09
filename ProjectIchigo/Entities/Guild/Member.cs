namespace ProjectIchigo.Entities;

internal class Member
{
    private string _SavedNickname { get; set; } = "";
    public string SavedNickname { get => _SavedNickname; set { _SavedNickname = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private DateTime _FirstJoinDate { get; set; } = DateTime.UnixEpoch;
    public DateTime FirstJoinDate { get => _FirstJoinDate; set { _FirstJoinDate = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private DateTime _LastLeaveDate { get; set; } = DateTime.UnixEpoch;
    public DateTime LastLeaveDate { get => _LastLeaveDate; set { _LastLeaveDate = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    public InviteTrackerMember InviteTracker { get; set; } = new();

    public ExperienceMember Experience { get; set; } = new();

    public List<MemberRole> MemberRoles { get; set; } = new();
}
