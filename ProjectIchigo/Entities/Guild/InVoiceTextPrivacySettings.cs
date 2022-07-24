namespace ProjectIchigo.Entities;

public class InVoiceTextPrivacySettings
{
    public InVoiceTextPrivacySettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _ClearTextEnabled { get; set; } = false;
    public bool ClearTextEnabled 
    { 
        get => _ClearTextEnabled; 
        set 
        { 
            _ClearTextEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "vc_privacy_clear", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _SetPermissionsEnabled { get; set; } = false;
    public bool SetPermissionsEnabled 
    { 
        get => _SetPermissionsEnabled; 
        set 
        { 
            _SetPermissionsEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "vc_privacy_perms", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}