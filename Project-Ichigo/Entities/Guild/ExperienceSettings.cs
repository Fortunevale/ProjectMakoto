namespace ProjectIchigo.Entities;

internal class ExperienceSettings
{
    private bool _UseExperience { get; set; } = false;
    public bool UseExperience { get => _UseExperience; set { _UseExperience = value; _ = Bot.DatabaseClient.SyncDatabase(); } }



    private bool _BoostXpForBumpReminder { get; set; } = false;
    public bool BoostXpForBumpReminder { get => _BoostXpForBumpReminder; set { _BoostXpForBumpReminder = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}
