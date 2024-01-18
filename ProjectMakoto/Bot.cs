// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Octokit;
using GenHTTP.Engine;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.StaticWebsites;

namespace ProjectMakoto;

public sealed class Bot
{
    #region Clients

    internal DatabaseClient DatabaseClient { get; set; }

    public DiscordShardedClient DiscordClient { get; internal set; }
    internal LavalinkSession LavalinkSession;

    internal ScoreSaberClient ScoreSaberClient { get; set; }
    public GoogleTranslateClient TranslationClient { get; internal set; }
    public ThreadJoinClient ThreadJoinClient { get; internal set; }
    public AbuseIpDbClient AbuseIpDbClient { get; internal set; }
    public MonitorClient MonitorClient { get; internal set; }
    public TokenInvalidatorRepository TokenInvalidator { get; internal set; }
    internal GitHubClient GithubClient { get; set; }
    internal ChartGeneration ChartsClient { get; set; }

    internal IServerHost WebServer { get; set; }

    #endregion Clients

    #region Plugins
    public IReadOnlyDictionary<string, BasePlugin> Plugins => this._Plugins.AsReadOnly();
    internal Dictionary<string, BasePlugin> _Plugins { get; set; } = [];

    public IReadOnlyDictionary<string, List<BasePluginCommand>> PluginCommands => this._PluginCommands.AsReadOnly();
    internal Dictionary<string, List<BasePluginCommand>> _PluginCommands { get; set; } = new();
    #endregion

    #region Util

    internal Translations LoadedTranslations { get; set; }

    public CountryCodes CountryCodes { get; internal set; }
    public LanguageCodes LanguageCodes { get; internal set; }
    internal IReadOnlyList<string> ProfanityList { get; set; }

    internal BumpReminderHandler BumpReminder { get; set; }
    internal ExperienceHandler ExperienceHandler { get; set; }
    public TaskWatcher Watcher { get; internal set; } = new();

    internal DatabaseDictionary<string, PhishingUrlEntry> PhishingHosts { get; set; }
    internal DatabaseDictionary<ulong, SubmittedUrlEntry> SubmittedHosts { get; set; }

    #endregion Util


    #region Bans

    internal DatabaseList<ulong> objectedUsers { get; set; }
    internal DatabaseDictionary<ulong, BanDetails> bannedUsers { get; set; }
    internal DatabaseDictionary<ulong, BanDetails> bannedGuilds { get; set; }

    internal DatabaseDictionary<ulong, BanDetails> globalBans { get; set; }
    internal SelfFillingDatabaseDictionary<GlobalNote> globalNotes { get; set; }

    #endregion Bans

    public Status status = new();
    internal SelfFillingDatabaseDictionary<Entities.Guild> Guilds = null;
    internal SelfFillingDatabaseDictionary<Entities.User> Users = null;

    internal string RawFetchedPrivacyPolicy = "";
    internal string Prefix { get; private set; } = ";;";

    internal async Task Init(string[] args)
    {
        _logger = LoggerClient.StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", CustomLogLevel.Info, DateTime.UtcNow.AddDays(-3), false);
        _logger.LogRaised += this.LogHandler;

        ScheduledTaskExtensions.TaskStarted += this.TaskStarted;
        UniversalExtensions.AttachLogger(_logger);

        RenderAsciiArt();

        this.status.RunningVersion = (File.Exists("LatestGitPush.cfg") ? await File.ReadAllLinesAsync("LatestGitPush.cfg") : new string[] { "Development-Build" })[0].Trim();
        _logger.LogInfo("Starting up Makoto {RunningVersion}..\n", this.status.RunningVersion);

        if (args.Contains("--debug"))
        {
            _logger.ChangeLogLevel(CustomLogLevel.Debug);
        }
        
        if (args.Contains("--debug2"))
        {
            _logger.ChangeLogLevel(CustomLogLevel.Debug2);
        }

        if (args.Contains("--trace"))
        {
            _logger.ChangeLogLevel(CustomLogLevel.Trace);
        }

        if (args.Contains("--trace2"))
        {
            _logger.ChangeLogLevel(CustomLogLevel.Trace2);
        }

        _logger.LogDebug("Environment Details\n\n" +
                "Dotnet Version: {Version}\n" +
                "OS & Version: {OSVersion}\n\n" +
                "OS 64x: {Is64BitOperatingSystem}\n" +
                "Process 64x: {Is64BitProcess}\n\n" +
                "MachineName: {MachineName}\n" +
                "UserName: {UserName}\n" +
                "UserDomain: {UserDomainName}\n\n" +
                "Current Directory: {CurrentDirectory}\n" +
                "Commandline: {Commandline}\n",
                Environment.Version,
                Environment.OSVersion,
                Environment.Is64BitOperatingSystem,
                Environment.Is64BitProcess,
                Environment.MachineName,
                Environment.UserName,
                Environment.UserDomainName,
                Environment.CurrentDirectory,
                Regex.Replace(Environment.CommandLine, @"(--token \S*)", ""));

        _ = Task.Run(() =>
        {
            UniversalExtensions.LoadAllReferencedAssemblies(AppDomain.CurrentDomain);
        });

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                await Util.Initializers.ConfigLoader.Load(this);
                this.ScoreSaberClient = new ScoreSaberClient(this);
                this.TranslationClient = new GoogleTranslateClient(this);
                this.ThreadJoinClient = new ThreadJoinClient();
                this.MonitorClient = new MonitorClient(this);
                this.AbuseIpDbClient = new AbuseIpDbClient(this);
                this.TokenInvalidator = new TokenInvalidatorRepository(this);
                this.ChartsClient = new ChartGeneration(this);
                this.GithubClient = new GitHubClient(new ProductHeaderValue("ProjectMakoto", this.status.RunningVersion))
                {
                    Credentials = new Credentials(this.status.LoadedConfig.Secrets.Github.Token)
                };

                await Task.WhenAll(Util.Initializers.ListLoader.Load(this), 
                                   Util.Initializers.TranslationLoader.Load(this), 
                                   Util.Initializers.PluginLoader.LoadPlugins(this),
                                   Util.Initializers.DependencyLoader.Load(this));

                _ = await DatabaseClient.InitializeDatabase(this);
                _ = BasePlugin.RaiseDatabaseInitialized(this);

                _ = Directory.CreateDirectory("WebServer");

                this.WebServer = Host.Create()
                                    .Port(this.status.LoadedConfig.WebServer.Port)
                                    .Console()
                                    .Defaults(true, false, false, true, false, false)
                                    .Handler(StaticWebsite.From(ResourceTree.FromDirectory("WebServer")))
                                    .Start();

                this.objectedUsers = new(this.DatabaseClient, "objected_users", "id", false);

                this.PhishingHosts = new(this.DatabaseClient, "scam_urls", "url", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new PhishingUrlEntry(this, id);
                });
                
                this.SubmittedHosts = new(this.DatabaseClient, "active_url_submissions", "messageid", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new SubmittedUrlEntry(this, id);
                });
                //this.PhishingHosts = new();

                this.Users = new(this.DatabaseClient, "users", "userid", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new Entities.User(this, id);
                });

                this.Guilds = new(this.DatabaseClient, "guilds", "serverid", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new Entities.Guild(this, id);
                });
                
                this.globalNotes = new(this.DatabaseClient, "globalnotes", "id", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new Entities.GlobalNote(this, id);
                });

                this.bannedUsers = new(this.DatabaseClient, "banned_users", "id", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new BanDetails(this, "banned_users", id);
                });

                this.bannedGuilds = new(this.DatabaseClient, "banned_guilds", "id", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new BanDetails(this, "banned_guilds", id);
                });
                
                this.globalBans = new(this.DatabaseClient, "globalbans", "id", this.DatabaseClient.mainDatabaseConnection, (id) =>
                {
                    return new BanDetails(this, "globalbans", id);
                });

                this.BumpReminder = new(this);
            }
            catch (Exception ex)
            {
                _logger.LogFatal("An exception occurred while initializing data", ex);
                await Task.Delay(5000);
                Environment.Exit((int)ExitCodes.FailedDatabaseLogin);
            }

            _ = new PhishingUrlHandler(this).UpdatePhishingUrlDatabase();
        }).Add(this).IsVital();

        await loadDatabase.Task.WaitAsync(TimeSpan.FromSeconds(600));

        var logInToDiscord = Task.Run(async () =>
        {
            _ = Task.Delay(60000).ContinueWith(t =>
            {
                if (!this.status.DiscordInitialized)
                {
                    _logger.LogError("An exception occurred while trying to log into discord: {0}", "The log in took longer than 60 seconds");
                    Environment.FailFast("An exception occurred while trying to log into discord: The log in took longer than 60 seconds");
                    return;
                }
            });

            await Util.Initializers.DisCatSharpExtensionsLoader.Load(this);

            _logger.LogInfo("Connecting and authenticating with Discord..");
            await this.DiscordClient.StartAsync();
            await Task.Delay(2000);
            _logger.LogInfo("Connected and authenticated with Discord as {User}.", this.DiscordClient.CurrentUser.GetUsernameWithIdentifier());

            this.status.DiscordInitialized = true;
            await BasePlugin.RaiseConnected(this);

            await Util.Initializers.PostLoginTaskLoader.Load(this);

            foreach (var plugin in this.Plugins)
                _ = plugin.Value.PostLoginInternalInit().Add(this);

            //foreach (var guild in this.DiscordClient.GetGuilds().Values)
            //    await this.DiscordClient.GetShard(guild.Id).BulkOverwriteGuildApplicationCommandsAsync(guild.Id, Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);

            //await this.DiscordClient.GetFirstShard().BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);

            _ = Task.Run(async () =>
            {
                if (this.status.LoadedConfig.DontModify.LastStartedVersion == this.status.RunningVersion)
                    return;

                this.status.LoadedConfig.DontModify.LastStartedVersion = this.status.RunningVersion;
                this.status.LoadedConfig.Save();

                var channel = await this.DiscordClient.GetFirstShard().GetChannelAsync(this.status.LoadedConfig.Channels.GithubLog);
                _ = await channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Color = EmbedColors.Success,
                    Title = $"Successfully updated to `{this.status.RunningVersion}`."
                });
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    this.status.TeamOwner = this.DiscordClient.CurrentApplication.Team.Owner.Id;
                    _logger.LogInfo("Set {TeamOwner} as owner of the bot", this.status.TeamOwner);

                    this.status._TeamMembers.AddRange(this.DiscordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                    _logger.LogInfo("Added {Count} users to administrator list", this.status.TeamMembers.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                }

                try
                {
                    if (this.DiscordClient.CurrentApplication.PrivacyPolicyUrl.IsNullOrWhiteSpace())
                        throw new Exception("No privacy policy was defined.");

                    this.RawFetchedPrivacyPolicy = await new HttpClient().GetStringAsync(this.DiscordClient.CurrentApplication.PrivacyPolicyUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred while trying to fetch the privacy policy", ex);
                }
            });

            _ = this.ProcessDeletionRequests().Add(this);
        }).Add(this).IsVital();

        while (!loadDatabase.Task.IsCompleted || !logInToDiscord.Task.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.Task.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the database.", loadDatabase.Task.Exception);
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.Task.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the discord client.", logInToDiscord.Task.Exception);
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.FailedDiscordLogin);
        }

        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            this.ExitApplication(true).Wait();
        };

        Console.CancelKeyPress += delegate
        {
            _logger.LogInfo("Exiting, please wait..");
            this.ExitApplication().Wait();
        };


        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (File.Exists("updated"))
                {
                    File.Delete("updated");
                    await this.ExitApplication();
                    return;
                }

                await Task.Delay(1000);
            }
        }).Add(this).IsVital();

        await Task.Delay(-1);
    }

    private static void RenderAsciiArt()
    {
        try
        {
            var ASCII = File.ReadAllText("Assets/ASCII.txt");
            Console.WriteLine();
            foreach (var b in ASCII)
            {
                switch (b)
                {
                    case 'g':
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case 'b':
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case 'r':
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case 'p':
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                    default:
                        Console.Write(b);
                        break;
                }
            }
            Console.WriteLine("\n\n");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to render ASCII art", ex);
        }

        Console.ResetColor();
    }

    internal Task<int> GetPrefix(DiscordMessage message)
    {
        return Task<int>.Run(() =>
        {
            //if (IsDev)
            //    if (!_status.TeamMembers.Any(x => x == message.Author.Id))
            //        return -1;

            var currentPrefix = this.Guilds.TryGetValue(message.GuildId ?? 0, out var guild) ? guild.PrefixSettings.Prefix : this.Prefix;

            var CommandStart = -1;

            if (!(guild?.PrefixSettings.PrefixDisabled ?? false))
                CommandStart = CommandsNextUtilities.GetStringPrefixLength(message, currentPrefix);

            if (CommandStart == -1)
                CommandStart = CommandsNextUtilities.GetMentionPrefixLength(message, this.DiscordClient.CurrentUser);

            return CommandStart;
        });
    }
    bool ExitCalled = false;

    internal async Task ExitApplication(bool Immediate = false)
    {
        _ = Task.Delay(Immediate ? TimeSpan.FromSeconds(10) : TimeSpan.FromMinutes(5)).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                Environment.Exit((int)ExitCodes.ExitTasksTimeout); // Fail-Safe in case the shutdown tasks lock up
                await Task.Delay(5000);
                Environment.FailFast(null);
            }
        });

        if (this.DatabaseClient.Disposed || this.ExitCalled) // When the Database Client has been disposed, the Exit Call has already been made.
            return;

        this.ExitCalled = true;

        _logger.LogInfo("Preparing to shut down Makoto..");

        _ = this.WebServer.Stop();

        foreach (var b in this.Plugins)
        {
            try
            {
                _logger.LogInfo("Shutting down '{0}'..", b.Value.Name);
                await b.Value.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to shutdown", ex, b.Value.Name);
            }
        }

        if (this.status.DiscordInitialized && !Immediate)
        {
            try
            {
                Stopwatch sw = new();
                sw.Start();

                if (!this.status.DiscordCommandsRegistered)
                    _logger.LogWarn("Startup is incomplete. Waiting for Startup to finish to shutdown..");

                while (!this.status.DiscordCommandsRegistered && sw.ElapsedMilliseconds < TimeSpan.FromMinutes(5).TotalMilliseconds)
                    await Task.Delay(500);

                await Util.Initializers.SyncTasks.ExecuteSyncTasks(this, this.DiscordClient);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            try
            {
                _logger.LogInfo("Closing Discord Client..");

                await this.DiscordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
                await this.DiscordClient.StopAsync();

                _logger.LogDebug("Closed Discord Client.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close Discord Client gracefully.", ex);
            }
        }

        await Task.Delay(500);
        _logger.LogInfo("Goodbye!");

        await Task.Delay(500);
        Environment.Exit(0);
        await Task.Delay(10000);
        Environment.FailFast("Failed to exit");
    }

    private async Task ProcessDeletionRequests()
    {
        _ = new Func<Task>(async () =>
        {
            _ = this.ProcessDeletionRequests().Add(this);
        }).CreateScheduledTask(DateTime.UtcNow.AddHours(24));

        lock (this.Users)
        {
            foreach (var b in this.Users)
            {
                if ((b.Value?.Data?.DeletionRequested ?? false) && b.Value?.Data?.DeletionRequestDate.GetTimespanUntil() < TimeSpan.Zero)
                {
                    _logger.LogInfo("Deleting profile of '{Key}'", b.Key);

                    _ = this.Users.Remove(b.Key);
                    this.objectedUsers.Add(b.Key);
                    foreach (var c in this.DiscordClient.GetGuilds().Where(x => x.Value.OwnerId == b.Key))
                    {
                        try
                        { _logger.LogInfo("Leaving guild '{guild}'..", c.Key); _ = c.Value.LeaveAsync().Add(this); }
                        catch { }
                    }
                }
            }
        }
    }

    internal Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        => Util.Initializers.SyncTasks.GuildDownloadCompleted(this, sender, e);

    internal void LogHandler(object? sender, LogMessageEventArgs e)
        => this.Watcher.LogHandler(this, sender, e);

    internal void TaskStarted(object? sender, Xorog.UniversalExtensions.EventArgs.ScheduledTaskStartedEventArgs e)
        => this.Watcher.TaskStarted(this, sender, e);
}