// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Plugins.Commands;

namespace ProjectMakoto.Plugins;

public abstract class BasePlugin
{
    public BasePlugin()
    {
        this._logger = Log._logger;
    }

    internal FileInfo LoadedFile { get; set; }

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

    /// <summary>
    /// The name of this plugin.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The description of this plugin.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// The plugin author's name.
    /// </summary>
    public abstract string Author { get; }

    /// <summary>
    /// The plugin author's discord id.
    /// </summary>
    public abstract ulong? AuthorId { get; }

    /// <summary>
    /// The current version of this plugin.
    /// </summary>
    public abstract SemVer Version { get; }

    /// <summary>
    /// The url to the github repo containing this plugin. Used for automated update checking.
    /// </summary>
    public virtual string UpdateUrl { get; }

    /// <summary>
    /// If the plugin is in a private repo, a login may be required.
    /// </summary>
    public virtual Credentials? UpdateUrlCredentials { get; }

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

    public void EnableCommandTranslations(ApplicationCommandsTranslationContext ctx)
    {
        return;
    }

    public object GetConfig()
        => (_bot.status.LoadedConfig.PluginData.TryGetValue(this.Name, out var val) ? val : null);

    public void WriteConfig(object configObject)
    {
        if (!_bot.status.LoadedConfig.PluginData.ContainsKey(this.Name))
            _bot.status.LoadedConfig.PluginData.Add(this.Name, null);

        _bot.status.LoadedConfig.PluginData[this.Name] = configObject;
        _bot.status.LoadedConfig.Save();
    }

    public bool CheckIfConfigExists() 
        => _bot.status.LoadedConfig.PluginData.ContainsKey(this.Name);

    internal async Task CheckForUpdates()
    {
        if (this.UpdateUrl is null)
            return;

        var regex = RegexTemplates.GitHubRepoUrl.Match(this.UpdateUrl);

        if (!regex.Success)
            throw new InvalidDataException("The provided url does not match a github repo url.");

        var Owner = regex.Groups[1].Value;
        var Repository = regex.Groups[2].Value;

        GitHubClient client = new(new ProductHeaderValue("ProjectMakoto"));

        if (this.UpdateUrlCredentials is not null)
            client.Credentials = this.UpdateUrlCredentials;

        try
        {
            var release = await client.Repository.Release.GetLatest(Owner, Repository);
            var latestVersion = new SemVer(release.TagName);
            var currentVersion = this.Version;

            if ((int)latestVersion > (int)currentVersion)
            {
                _logger.LogWarn("Plugin '{PluginName}' has an update available. The installed version is '{CurrentVersion}' and the latest version is '{LatestVersion}'.", this.Name, currentVersion, latestVersion);

                if (UpdateUrlCredentials is not null)
                {
                    _logger.LogInfo("Plugin '{PluginName}' has a private repository. Downloading latest version to 'UpdatedPlugins' Directory..", this.Name);
                    Directory.CreateDirectory("UpdatedPlugins");
                    HttpClient downloadClient = new();

                    var asset = release.Assets.First(x => x.Name.EndsWith(".dll"));

                    using (var fileStream = new FileStream($"UpdatedPlugins/{asset.Name}", FileMode.Create, FileAccess.ReadWrite))
                    {
                        var downloadStream = await downloadClient.GetStreamAsync(asset.BrowserDownloadUrl);
                        await downloadStream.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    _logger.LogWarn("You can find the update for '{PluginName}' at '{LatestReleaseUrl}'", this.Name, release.HtmlUrl);
                }
            }
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("The repository of '{PluginName}' could not be found at '{RepoUrl}', the repo is private or the credentials are outdated.", this.Name, this.UpdateUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not check for a new version of '{PluginName}'", ex, this.Name);
        }
    }
}
