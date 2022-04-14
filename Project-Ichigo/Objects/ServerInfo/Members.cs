namespace Project_Ichigo.Objects;

internal class Members
{
    private DateTime _FirstJoinDate { get; set; } = DateTime.UnixEpoch;
    public DateTime FirstJoinDate { get => _FirstJoinDate; set { _FirstJoinDate = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private DateTime _LastLeaveDate { get; set; } = DateTime.UnixEpoch;
    public DateTime LastLeaveDate { get => _LastLeaveDate; set { _LastLeaveDate = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private long _Experience { get; set; } = 1;
    public long Experience { get => _Experience; set { _Experience = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private long _Level { get; set; } = 1;
    public long Level { get
        {
            if (_Level <= 0)
                return 1;

            return _Level;
        } set { _Level = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private DateTime _Last_Message { get; set; } = DateTime.UnixEpoch;
    public DateTime Last_Message { get => _Last_Message; set { _Last_Message = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    public List<MembersRole> MemberRoles { get; set; } = new();
}
