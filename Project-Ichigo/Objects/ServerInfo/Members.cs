namespace Project_Ichigo.Objects;

internal class Members
{
    private long _Experience { get; set; } = 1;
    public long Experience { get => _Experience; set { _Experience = value; _ = Bot._databaseHelper.SyncDatabase(); } }



    private long _Level { get; set; } = 1;
    public long Level { get
        {
            if (_Level <= 0)
                return 1;

            return _Level;
        } set { _Level = value; _ = Bot._databaseHelper.SyncDatabase(); } }


    private DateTime _Last_Message { get; set; } = DateTime.UnixEpoch;
    public DateTime Last_Message { get => _Last_Message; set { _Last_Message = value; _ = Bot._databaseHelper.SyncDatabase(); } }
}
