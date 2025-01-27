// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;
using ProjectMakoto.Plugins.EventArgs;
using ProjectMakoto.Util.Initializers;
using User = ProjectMakoto.Entities.User;

namespace ProjectMakoto.Plugins;

public abstract class BasePlugin
{
    public BasePlugin()
    {
        this._logger = new(Log.Logger, this);
    }

    /// <summary>
    /// 1
    /// </summary>
    internal static int CurrentApiVersion = 1;

    /// <summary>
    /// The file this plugin was loaded from.
    /// </summary>
    internal FileInfo LoadedFile { get; set; }

    /// <summary>
    /// Whether this plugin has translations enabled.
    /// </summary>
    internal bool UsesTranslations { get; set; } = false;

    /// <summary>
    /// A list of all the tables this plugin has access to.
    /// </summary>
    internal List<string> AllowedTables { get; set; } = new();

    /// <summary>
    /// Whether this plugin is official or not.
    /// </summary>
    internal bool OfficialPlugin { get; set; } = new();

    /// <summary>
    /// Makoto Instance
    /// </summary>
    public Bot Bot { get; set; }

    /// <summary>
    /// Allows you to log events.
    /// </summary>
    public PluginLoggerClient _logger { get; internal set; }

    /// <summary>
    /// Your Plugin's translations, load via <see cref="LoadTranslations"/>.
    /// </summary>
    public ITranslations Translations { get; internal set; }

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

    #region Events

    /// <summary>
    /// Raised before login to discord takes place. Useful for registering DisCatSharp extensions.
    /// </summary>
    public event EventHandler<PreLoginEventArgs> PreLogin;
    internal static Task RaisePreLogin(Bot bot, DiscordShardedClient client) 
        => Task.Run(() => CallEvent(bot, bot.Plugins?.Select(x => x.Value.PreLogin ), new PreLoginEventArgs(client)));
    
    /// <summary>
    /// Raised on first successful log in to discord.
    /// </summary>
    public event EventHandler<System.EventArgs> Connected;
    internal static Task RaiseConnected(Bot bot) 
        => Task.Run(() => CallEvent(bot, bot.Plugins?.Select(x => x.Value.Connected), System.EventArgs.Empty));

    /// <summary>
    /// Raised on when database is initialized.
    /// </summary>
    public event EventHandler<System.EventArgs> DatabaseInitialized;
    internal static Task RaiseDatabaseInitialized(Bot bot) 
        => Task.Run(() => CallEvent(bot, bot.Plugins?.Select(x => x.Value.DatabaseInitialized), System.EventArgs.Empty));

    /// <summary>
    /// Raised before sync tasks are ran.
    /// </summary>
    public event EventHandler<SyncTaskEventArgs> PreSyncTasksExecution;
    internal static Task RaisePreSyncTasksExecution(Bot bot, IEnumerable<DiscordGuild> discordGuilds) 
        => Task.Run(() => CallEvent(bot, bot.Plugins?.Select(x => x.Value.PreSyncTasksExecution), new(discordGuilds)));

    /// <summary>
    /// Raised after sync tasks are ran.
    /// </summary>
    public event EventHandler<SyncTaskEventArgs> PostSyncTasksExecution;
    internal static Task RaisePostSyncTasksExecution(Bot bot, IEnumerable<DiscordGuild> discordGuilds)
        => Task.Run(() => CallEvent(bot, bot.Plugins?.Select(x => x.Value.PostSyncTasksExecution), new(discordGuilds)));

    #endregion

    #region Plugin Identity
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
    /// Loads the author from discord upon launch, null if failed to fetch.
    /// </summary>
    public DiscordUser? AuthorUser { get; internal set; }

    /// <summary>
    /// The current version of this plugin.
    /// </summary>
    public abstract SemVer Version { get; }

    /// <summary>
    /// The currently supported PluginApis. Current Plugin Api is <inheritdoc cref="BasePlugin.CurrentApiVersion"/>
    /// <para>Gets changed every breaking change.</para>
    /// <code> = [ 1, 2 ]; // example </code>
    /// </summary>
    public abstract int[] SupportedPluginApis { get; }

    /// <summary>
    /// The url to the github repo containing this plugin. Used for automated update checking.
    /// </summary>
    public virtual string UpdateUrl { get; }

    /// <summary>
    /// If the plugin is in a private repo, a login may be required.
    /// </summary>
    public virtual Credentials? UpdateUrlCredentials { get; }
    #endregion

    #region Plugin Init Logic
    /// <summary>
    /// Called upon loading dll.
    /// </summary>
    /// <param name="bot">The loading Makoto instance.</param>
    internal void Load(Bot bot)
    {
        this.Bot = bot;
        _ = this.Initialize();
    }

    /// <summary>
    /// Called when plugin was loaded into memory.
    /// </summary>
    /// <returns>The plugin</returns>
    public abstract BasePlugin Initialize();

    /// <summary>
    /// Called after registering built-in commands.
    /// </summary>
    /// <returns>A list of all commands the plugin wants to register. (An empty list if none.)</returns>
    public virtual async Task<IEnumerable<MakotoModule>> RegisterCommands()
    {
        return new List<MakotoModule>();
    }

    /// <summary>
    /// Called when initializing the database connection. Allows you to register your own database tables.
    /// </summary>
    /// <returns>A list of all tables the plugin wants to register (or <see langword="null"/>).</returns>
    public virtual async Task<IEnumerable<Type>?> RegisterTables()
    {
        return null;
    }

    /// <summary>
    /// Allows you to define a translation file. Return <see langword="null"/> or empty string if none is present.
    /// </summary>
    /// <returns>A tuple of a string responsible for the path of the json and type to deserialize the json into.</returns>
    public virtual (string? path, Type? type) LoadTranslations()
    {
        return (null, null);
    }

    /// <summary>
    /// Called when Makoto is shuttin down.
    /// </summary>
    /// <returns></returns>
    public virtual async Task Shutdown()
    {
        return;
    }
    #endregion

    #region Config Logic
    /// <summary>
    /// Gets your plugin's config object.
    /// </summary>
    /// <returns>The previously saved config or <see langword="null"/> if none exists yet.</returns>
    public object GetConfig()
        => (this.Bot.status.LoadedConfig.PluginData.TryGetValue(this.Name, out var val) ? val : null);

    /// <summary>
    /// Writes your plugin's config to makoto's config.
    /// </summary>
    /// <param name="configObject"></param>
    public void WriteConfig(object configObject)
    {
        if (!this.Bot.status.LoadedConfig.PluginData.ContainsKey(this.Name))
            this.Bot.status.LoadedConfig.PluginData.Add(this.Name, null);

        this.Bot.status.LoadedConfig.PluginData[this.Name] = configObject;
        this.Bot.status.LoadedConfig.Save();
    }

    /// <summary>
    /// Checks whether a config already exists.
    /// </summary>
    /// <returns>Whether or not the a config has already been created.</returns>
    public bool CheckIfConfigExists()
        => this.Bot.status.LoadedConfig.PluginData.ContainsKey(this.Name);
    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if a user has objected to having their data processed.
    /// </summary>
    /// <param name="id">The user's id</param>
    /// <returns></returns>
    public bool HasUserObjected(ulong id)
        => this.Bot.objectedUsers?.Contains(id) ?? false;

    /// <inheritdoc cref="HasUserObjected(ulong)"/>
    /// <param name="user">The user.</param>
    public bool HasUserObjected(DiscordUser user)
        => this.HasUserObjected(user?.Id ?? 0);

    /// <summary>
    /// Checks if a user has been banned from using this bot.
    /// </summary>
    /// <param name="id">The user's id</param>
    /// <returns></returns>
    public bool IsUserBanned(ulong id)
        => this.Bot.bannedUsers?.ContainsKey(id) ?? false;

    /// <inheritdoc cref="HasUserObjected(ulong)"/>
    /// <param name="user">The user.</param>
    public bool IsUserBanned(DiscordUser user)
        => this.IsUserBanned(user?.Id ?? 0);

    /// <summary>
    /// Checks if a user has objected to having their data processed.
    /// </summary>
    /// <param name="id">The user's id</param>
    /// <returns></returns>
    public bool IsGuildBanned(ulong id)
        => this.Bot.bannedGuilds?.ContainsKey(id) ?? false;

    /// <inheritdoc cref="HasUserObjected(ulong)"/>
    /// <param name="guild">The guild.</param>
    public bool IsGuildBanned(DiscordGuild guild)
        => this.IsGuildBanned(guild?.Id ?? 0);

    #endregion

    #region Internal Logic
    /// <summary>
    /// Calls an event for all plugin instances.
    /// </summary>
    /// <typeparam name="T">The type of event</typeparam>
    /// <param name="bot">The event sender</param>
    /// <param name="eventInstances">All event instances</param>
    /// <param name="args">The arguments</param>
    /// <returns></returns>
    private static Task CallEvent<T>(Bot bot, IEnumerable<EventHandler<T>?> eventInstances, T? args)
    {
        if (eventInstances is null)
            return Task.CompletedTask;

        foreach (var e in eventInstances)
        {
            if (e is null)
                continue;

            try
            {
                e.Invoke(bot, args);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to run event handler");
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Enables AppCommand Translations if plugin provides any.
    /// </summary>
    /// <param name="ctx"></param>
    internal void EnableCommandTranslations(ApplicationCommandsTranslationContext ctx)
    {
        DisCatSharpExtensionsLoader.GetCommandTranslations(ctx);

        if (!this.UsesTranslations)
        {
            ctx.AddSingleTranslation(null);
            ctx.AddGroupTranslation(null);
        }

        return;
    }

    internal async Task PostLoginInternalInit()
    {
        this._logger.LogDebug("Performing Post-Login tasks for {plugin}", this.Name);

        if (this.Bot.DiscordClient.GetFirstShard().TryGetUser(this.AuthorId ?? 0, out var fetchedAuthor, true))
            this.AuthorUser = fetchedAuthor;
    }

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
        catch (Octokit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            this._logger.LogError("The repository could not be found at '{RepoUrl}', is the repo private, the credentials outdated or no release published?", this.UpdateUrl);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Could not check for a new version");
        }
    }

    #endregion
}
