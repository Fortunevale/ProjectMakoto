namespace Project_Ichigo.Objects;

internal class ScoreSaber
{
    private ulong _Id { get; set; } = 0;
    public ulong Id { get => _Id; set { _Id = value; _ = Bot._databaseHelper.SyncDatabase(); } }
}
