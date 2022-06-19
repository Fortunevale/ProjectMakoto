namespace ProjectIchigo.Entities;

internal class InVoiceTextPrivacySettings
{
    private bool _ClearTextEnabled { get; set; } = false;
    public bool ClearTextEnabled { get => _ClearTextEnabled; set { _ClearTextEnabled = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _SetPermissionsEnabled { get; set; } = false;
    public bool SetPermissionsEnabled { get => _SetPermissionsEnabled; set { _SetPermissionsEnabled = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}