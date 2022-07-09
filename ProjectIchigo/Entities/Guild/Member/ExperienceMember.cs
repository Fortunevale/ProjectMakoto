namespace ProjectIchigo.Entities;

internal class ExperienceMember
{
    private DateTime _Last_Message { get; set; } = DateTime.UnixEpoch;
    public DateTime Last_Message { get => _Last_Message; set { _Last_Message = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private long _Points { get; set; } = 1;
    public long Points { get => _Points; set { _Points = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private long _Level { get; set; } = 1;
    public long Level
    {
        get
        {
            if (_Level <= 0)
                return 1;

            return _Level;
        }
        set { _Level = value; _ = Bot.DatabaseClient.SyncDatabase(); }
    }
}
