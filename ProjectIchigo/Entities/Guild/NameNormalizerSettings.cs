namespace ProjectIchigo.Entities;

public class NameNormalizerSettings
{
    public NameNormalizerSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _NameNormalizerEnabled { get; set; } = false;
    public bool NameNormalizerEnabled 
    { 
        get => _NameNormalizerEnabled; 
        set 
        { 
            _NameNormalizerEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "normalizenames", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    public bool NameNormalizerRunning = false;
}
