namespace Project_Ichigo.Objects;

internal class ExperienceUserSettings
{
    private bool _DirectMessageOptOut { get; set; } = false;
    public bool DirectMessageOptOut { get => _DirectMessageOptOut; set { _DirectMessageOptOut = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}
