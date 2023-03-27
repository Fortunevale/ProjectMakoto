using ProjectMakoto.Entities.Plugins.Commands;

namespace ProjectMakoto.Plugins;

public abstract class BasePlugin
{
    public BasePlugin()
    {
        this._logger = Log._logger;
    }

    public Bot _bot { get; set; }
    public Logger _logger { get; set; }
    public ApplicationCommandsExtension DiscordCommandsModule { get; set; }
    
    public bool DiscordInitialized
        => _bot.status.DiscordInitialized;
    
    public bool DiscordGuildDownloadCompleted
        => _bot.status.DiscordGuildDownloadCompleted;
    
    public bool DiscordCommandsRegistered
        => _bot.status.DiscordCommandsRegistered;

    public bool DatabaseInitialized
        => _bot.status.DatabaseInitialized;

    public bool DatabaseInitialLoadCompleted
        => _bot.status.DatabaseInitialLoadCompleted;

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Author { get; }
    public abstract ulong? AuthorId { get; }
    public abstract SemVer Version { get; }

    public void Load(Bot bot)
    {
        this._bot = bot;
        this.Initialize();

        _ = _ = Task.Run(async () =>
        {
            while (!DiscordInitialized)
            {
                await Task.Delay(1000);
            }

            this.DiscordCommandsModule = bot.discordClient.GetApplicationCommands();
        });
    }

    public abstract BasePlugin Initialize();

    public virtual async Task<List<BasePluginCommand>> RegisterCommands()
    {
        return new List<BasePluginCommand>();
    }

    public virtual async Task Shutdown()
    {
        return;
    }

    public object GetConfig(string configName)
        => (_bot.status.LoadedConfig.PluginData.TryGetValue(configName, out var val) ? val : null);

    public void WriteConfig(string configName, object configObject)
    {
        if (!_bot.status.LoadedConfig.PluginData.ContainsKey(configName))
            _bot.status.LoadedConfig.PluginData.Add(configName, null);

        _bot.status.LoadedConfig.PluginData[configName] = configObject;
        _bot.status.LoadedConfig.Save();
    }

    public bool CheckIfConfigExists(string configName) 
        => _bot.status.LoadedConfig.PluginData.ContainsKey(configName);
}
