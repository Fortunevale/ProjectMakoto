namespace ProjectIchigo.Entities;

public class TranslationSettings
{
    public TranslationSettings(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private string _LastGoogleSource { get; set; } = "";
    public string LastGoogleSource
    {
        get => _LastGoogleSource;
        set
        {
            _LastGoogleSource = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "last_google_source", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _LastGoogleTarget { get; set; } = "";
    public string LastGoogleTarget
    {
        get => _LastGoogleTarget;
        set
        {
            _LastGoogleTarget = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "last_google_target", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private string _LastLibreTranslateSource { get; set; } = "";
    public string LastLibreTranslateSource
    {
        get => _LastLibreTranslateSource;
        set
        {
            _LastLibreTranslateSource = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "last_libretranslate_source", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _LastLibreTranslateTarget { get; set; } = "";
    public string LastLibreTranslateTarget
    {
        get => _LastLibreTranslateTarget;
        set
        {
            _LastLibreTranslateTarget = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "last_libretranslate_target", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
