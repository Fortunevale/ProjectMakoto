namespace Project_Ichigo.Objects;

internal class Members
{
    private long _Experience { get; set; } = 1;
    public long Experience { get => _Experience; set { _Experience = value; _ = Bot._databaseHelper.SyncDatabase(); } }



    private long _Level { get; set; } = 1;
    public long Level { get => _Level; set { _Level = value; _ = Bot._databaseHelper.SyncDatabase(); } }
}
