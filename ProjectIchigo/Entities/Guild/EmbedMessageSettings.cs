namespace ProjectIchigo.Entities;

public class EmbedMessageSettings
{
    public EmbedMessageSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _UseEmbedding { get; set; } = false;
    public bool UseEmbedding
    {
        get => _UseEmbedding;
        set
        {
            _UseEmbedding = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "embed_messages", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
