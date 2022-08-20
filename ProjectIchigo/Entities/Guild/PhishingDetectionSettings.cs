namespace ProjectIchigo.Entities;

public class PhishingDetectionSettings
{
    public PhishingDetectionSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _DetectPhishing { get; set; } = true;
    public bool DetectPhishing 
    { 
        get => _DetectPhishing; 
        set 
        { 
            _DetectPhishing = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "phishing_detect", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private bool _WarnOnRedirect { get; set; } = false;
    public bool WarnOnRedirect 
    { 
        get => _WarnOnRedirect; 
        set 
        { 
            _WarnOnRedirect = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "phishing_warnonredirect", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private PhishingPunishmentType _PunishmentType { get; set; } = PhishingPunishmentType.KICK;
    public PhishingPunishmentType PunishmentType 
    { 
        get => _PunishmentType; 
        set 
        { 
            _PunishmentType = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "phishing_type", Convert.ToInt32(value), Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private string _CustomPunishmentReason { get; set; } = "%R";
    public string CustomPunishmentReason 
    { 
        get => _CustomPunishmentReason; 
        set 
        { 
            _CustomPunishmentReason = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "phishing_reason", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private TimeSpan _CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
    public TimeSpan CustomPunishmentLength 
    { 
        get => _CustomPunishmentLength; 
        set 
        { 
            _CustomPunishmentLength = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "phishing_time", Convert.ToInt64(value.TotalSeconds), Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
