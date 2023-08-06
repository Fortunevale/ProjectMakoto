// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;
using ProjectMakoto.Entities.Plugins.Commands;

namespace ProjectMakoto.Plugins;

public abstract class BasePlugin
{
    public BasePlugin()
    {
        this._logger = new(Log._logger, this);
    }

    internal FileInfo LoadedFile { get; set; }

    public Bot Bot { get; set; }

    /// <summary>
    /// Allows you to log events.
    /// </summary>
    public PluginLoggerClient _logger { get; set; }

    /// <summary>
    /// Whether the client logged into discord.
    /// </summary>
    public bool DiscordInitialized
        => this.Bot.status.DiscordInitialized;

    /// <summary>
    /// Whether the guild download has been completed.
    /// </summary>
    public bool DiscordGuildDownloadCompleted
        => this.Bot.status.DiscordGuildDownloadCompleted;

    /// <summary>
    /// Whether the commands have been registered.
    /// </summary>
    public bool DiscordCommandsRegistered
        => this.Bot.status.DiscordCommandsRegistered;

    /// <summary>
    /// Whether the database connection has been established.
    /// </summary>
    public bool DatabaseInitialized
        => this.Bot.status.DatabaseInitialized;

    /// <summary>
    /// Whether the database content has loaded.
    /// </summary>
    public bool DatabaseInitialLoadCompleted
        => this.Bot.status.DatabaseInitialLoadCompleted;

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
        this.Bot = bot;
        _ = this.Initialize();

        _ = _ = Task.Run(async () =>
        {
            while (!this.DiscordInitialized)
            {
                await Task.Delay(1000);
            }
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
        => (this.Bot.status.LoadedConfig.PluginData.TryGetValue(this.Name, out var val) ? val : null);

    public void WriteConfig(object configObject)
    {
        if (!this.Bot.status.LoadedConfig.PluginData.ContainsKey(this.Name))
            this.Bot.status.LoadedConfig.PluginData.Add(this.Name, null);

        this.Bot.status.LoadedConfig.PluginData[this.Name] = configObject;
        this.Bot.status.LoadedConfig.Save();
    }

    public bool CheckIfConfigExists()
        => this.Bot.status.LoadedConfig.PluginData.ContainsKey(this.Name);

    internal async Task CheckForUpdates()
    {
        if (this.UpdateUrl is null)
            return;

        var regex = RegexTemplates.GitHubRepoUrl.Match(this.UpdateUrl);

        if (!regex.Success)
            throw new InvalidDataException("The provided url does not match a github repo url.");

        var Owner = regex.Groups[1].Value;
        var Repository = regex.Groups[2].Value;

        GitHubClient client = new(new ProductHeaderValue("ProjectMakoto", this.Bot.status.RunningVersion));

        if (this.UpdateUrlCredentials is not null)
            client.Credentials = this.UpdateUrlCredentials;

        try
        {
            var release = await client.Repository.Release.GetLatest(Owner, Repository);
            var latestVersion = new SemVer(release.TagName);
            var currentVersion = this.Version;

            if ((int)latestVersion > (int)currentVersion)
            {
                this._logger.LogWarn("Update found. The installed version is '{CurrentVersion}' and the latest version is '{LatestVersion}'.", currentVersion, latestVersion);

                if (this.UpdateUrlCredentials is not null)
                {
                    this._logger.LogInfo("Private repository detected. Downloading latest version to 'UpdatedPlugins' Directory..");
                    _ = Directory.CreateDirectory("UpdatedPlugins");
                    HttpClient downloadClient = new();

                    var asset = release.Assets.First(x => x.Name.EndsWith(".dll"));

                    using (var fileStream = new FileStream($"UpdatedPlugins/{asset.Name}", System.IO.FileMode.Create, FileAccess.ReadWrite))
                    {
                        var downloadStream = await downloadClient.GetStreamAsync(asset.BrowserDownloadUrl);
                        await downloadStream.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    this._logger.LogWarn("You can download the update at '{LatestReleaseUrl}'", release.HtmlUrl);
                }
            }
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            this._logger.LogError("The repository could not be found at '{RepoUrl}', is the repo private, the credentials outdated or no release published?", this.UpdateUrl);
        }
        catch (Exception ex)
        {
            this._logger.LogError("Could not check for a new version", ex);
        }
    }
}
