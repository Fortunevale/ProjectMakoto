namespace ProjectIchigo.Entities;

internal class NameNormalizerSettings
{
    private bool _NameNormalizerEnabled { get; set; } = false;
    public bool NameNormalizerEnabled { get => _NameNormalizerEnabled; set { _NameNormalizerEnabled = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    public bool NameNormalizerRunning = false;
}
