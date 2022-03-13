namespace Project_Ichigo.Objects;

internal class ExperienceSettings
{
    private bool _UseExperience { get; set; } = true;
    public bool UseExperience { get => _UseExperience; set { _UseExperience = value; _ = Bot._databaseHelper.SyncDatabase(); } }
}
