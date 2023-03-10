namespace ProjectMakoto.Entities;

public abstract class BasePlugin
{
    public BasePlugin(Bot bot)
    {
        this._bot = bot;
        this._logger = Log._logger;
        this.DiscordCommandsModule = bot.discordClient.GetApplicationCommands();
    }

    public Bot _bot { get; set; }
    public Logger _logger { get; set; }
    public ApplicationCommandsExtension DiscordCommandsModule { get; set; }

    public abstract string Name { get; set; }
    public abstract string Description { get; set; }

    public abstract BasePlugin Initialize();

    public virtual async Task RegisterCommands(SharedCommandContext ctx)
    {
        return;
    }
}
