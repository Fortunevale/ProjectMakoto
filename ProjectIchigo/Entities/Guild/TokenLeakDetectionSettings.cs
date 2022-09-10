namespace ProjectIchigo.Entities;

public class TokenLeakDetectionSettings
{
    public TokenLeakDetectionSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }

    private bool _DetectTokens { get; set; } = true;
    public bool DetectTokens
    {
        get => _DetectTokens;
        set
        {
            _DetectTokens = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "tokens_detect", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
