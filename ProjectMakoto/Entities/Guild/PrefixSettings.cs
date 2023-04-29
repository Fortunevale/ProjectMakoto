namespace ProjectMakoto.Entities;
public class PrefixSettings
{
    public PrefixSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }

    private string _Prefix { get; set; } = "";
    public string Prefix
    {
        get => _Prefix.IsNullOrWhiteSpace() ? Parent._bot.Prefix : _Prefix; set
        {
            _Prefix = value.IsNullOrWhiteSpace() ? Parent._bot.Prefix : value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "prefix", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _PrefixDisabled { get; set; } = false;
    public bool PrefixDisabled
    {
        get => _PrefixDisabled; set
        {
            _PrefixDisabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "prefix_disabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
