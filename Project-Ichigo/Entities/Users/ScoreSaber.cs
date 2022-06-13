namespace ProjectIchigo.Entities;

internal class ScoreSaber
{
    private ulong _Id { get; set; } = 0;
    public ulong Id { get => _Id; set { _Id = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}
