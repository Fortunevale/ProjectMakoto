namespace ProjectIchigo.Entities;

public class ExperienceSettings
{
    public ExperienceSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _UseExperience { get; set; } = false;
    public bool UseExperience 
    { 
        get => _UseExperience; 
        set 
        { 
            _UseExperience = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "experience_use", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private bool _BoostXpForBumpReminder { get; set; } = false;
    public bool BoostXpForBumpReminder 
    { 
        get => _BoostXpForBumpReminder; 
        set 
        { 
            _BoostXpForBumpReminder = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "experience_boost_bumpreminder", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
