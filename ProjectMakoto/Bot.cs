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

namespace ProjectMakoto;

public sealed class Bot
{
    #region Clients

    internal static DatabaseClient DatabaseClient { get; set; }
    public DatabaseClient databaseClient
        => Bot.DatabaseClient;

    public DiscordClient discordClient { get; internal set; }
    internal LavalinkNodeConnection LavalinkNodeConnection;

    internal ScoreSaberClient scoreSaberClient { get; set; }
    public GoogleTranslateClient translationClient { get; internal set; }
    public ThreadJoinClient threadJoinClient { get; internal set; }
    public AbuseIpDbClient abuseIpDbClient { get; internal set; }
    public MonitorClient monitorClient { get; internal set; }
    internal GitHubClient githubClient { get; set; }

    public IReadOnlyDictionary<string, BasePlugin> Plugins
        => _Plugins.AsReadOnly();
    internal Dictionary<string, BasePlugin> _Plugins { get; set; } = new();

    public IReadOnlyDictionary<string, List<BasePluginCommand>> PluginCommands
        => _PluginCommands.AsReadOnly();
    internal Dictionary<string, List<BasePluginCommand>> _PluginCommands { get; set; } = new();

    #endregion Clients


    #region Util

    internal Translations loadedTranslations { get; set; }

    public CountryCodes countryCodes { get; internal set; }
    public LanguageCodes languageCodes { get; internal set; }
    internal IReadOnlyList<string> profanityList { get; set; }

    internal BumpReminder bumpReminder { get; set; }
    internal ExperienceHandler experienceHandler { get; set; }
    public TaskWatcher watcher { get; internal set; } = new();
    internal Dictionary<ulong, UserUpload> uploadInteractions { get; set; } = new();
    internal Dictionary<string, PhishingUrlEntry> phishingUrls = new();
    internal Dictionary<ulong, SubmittedUrlEntry> submittedUrls = new();

    #endregion Util


    #region Bans

    internal List<ulong> objectedUsers = new();
    internal Dictionary<ulong, BlacklistEntry> bannedUsers = new();
    internal Dictionary<ulong, BlacklistEntry> bannedGuilds = new();

    internal Dictionary<ulong, PhishingSubmissionBanDetails> phishingUrlSubmissionUserBans = new();
    internal Dictionary<ulong, PhishingSubmissionBanDetails> phishingUrlSubmissionGuildBans = new();

    internal Dictionary<ulong, GlobalBanDetails> globalBans = new();
    internal Dictionary<ulong, List<GlobalBanDetails>> globalNotes = new();

    #endregion Bans


    public Status status = new();
    internal GuildDictionary guilds = null;
    internal UserDictionary users = null;

    internal string RawFetchedPrivacyPolicy = "";
    internal string Prefix { get; private set; } = ";;";

    internal async Task Init(string[] args)
    {
        _logger = LoggerClient.StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", CustomLogLevel.Info, DateTime.UtcNow.AddDays(-3), false);
        _logger.LogRaised += LogHandler;

        UniversalExtensions.AttachLogger(_logger);

        try
        {
            string ASCII = File.ReadAllText("Assets/ASCII.txt");
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

        status.RunningVersion = (File.Exists("LatestGitPush.cfg") ? File.ReadLines("LatestGitPush.cfg") : new List<string> { "Development-Build" }).ToList()[0].Trim();

        _logger.LogInfo("Starting up Makoto {RunningVersion}..\n", status.RunningVersion);

        if (args.Contains("--debug"))
        {
            _logger.ChangeLogLevel(CustomLogLevel.Debug);
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

        UniversalExtensions.LoadAllReferencedAssemblies(AppDomain.CurrentDomain);

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                await Util.Initializers.ConfigLoader.Load(this);

                this.users = new(this);
                this.guilds = new(this);
                await DatabaseClient.InitializeDatabase(this);

                this.bumpReminder = new(this);

                this.scoreSaberClient = ScoreSaberClient.InitializeScoresaber();
                this.translationClient = GoogleTranslateClient.Initialize();
                this.threadJoinClient = ThreadJoinClient.Initialize();

                await Util.Initializers.ListLoader.Load(this);
                await Util.Initializers.TranslationLoader.Load(this);
                await Util.Initializers.PluginLoader.LoadPlugins(this);

                this.monitorClient = new MonitorClient(this);
                this.abuseIpDbClient = new AbuseIpDbClient(this);

                this.githubClient = new GitHubClient(new ProductHeaderValue("ProjectMakoto", status.RunningVersion));
                this.githubClient.Credentials = new Credentials(this.status.LoadedConfig.Secrets.Github.Token);

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.LoadValuesFromDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogFatal("An exception occurred while initializing data", ex);
                await Task.Delay(5000);
                Environment.Exit((int)ExitCodes.FailedDatabaseLogin);
            }

            _ = new PhishingUrlUpdater(this).UpdatePhishingUrlDatabase();
        }).Add(watcher).IsVital();

        await loadDatabase.task.WaitAsync(TimeSpan.FromSeconds(600));

        var logInToDiscord = Task.Run(async () =>
        {
            _ = Task.Delay(60000).ContinueWith(t =>
            {
                if (!status.DiscordInitialized)
                {
                    _logger.LogError("An exception occurred while trying to log into discord: {0}", "The log in took longer than 60 seconds");
                    Environment.Exit((int)ExitCodes.FailedDiscordLogin);
                    return;
                }
            });

            await Util.Initializers.DisCatSharpExtensionsLoader.Load(this, args);

            _logger.LogInfo("Connecting and authenticating with Discord..");
            await this.discordClient.ConnectAsync();
            _logger.LogInfo("Connected and authenticated with Discord.");

            this.status.DiscordInitialized = true;

            _ = Task.Run(async () =>
            {
                if (this.status.LoadedConfig.DontModify.LastStartedVersion == status.RunningVersion)
                    return;

                this.status.LoadedConfig.DontModify.LastStartedVersion = status.RunningVersion;
                this.status.LoadedConfig.Save();

                var channel = await this.discordClient.GetChannelAsync(this.status.LoadedConfig.Channels.GithubLog);
                await channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Color = EmbedColors.Success,
                    Title = $"Successfully updated to `{status.RunningVersion}`."
                });
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    this.status.TeamOwner = this.discordClient.CurrentApplication.Team.Owner.Id;
                    _logger.LogInfo("Set {TeamOwner} as owner of the bot", this.status.TeamOwner);

                    this.status._TeamMembers.AddRange(this.discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                    _logger.LogInfo("Added {Count} users to administrator list", this.status.TeamMembers.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                }

                try
                {
                    if (this.discordClient.CurrentApplication.PrivacyPolicyUrl.IsNullOrWhiteSpace())
                        throw new Exception("No privacy policy was defined.");

                    this.RawFetchedPrivacyPolicy = await new HttpClient().GetStringAsync(this.discordClient.CurrentApplication.PrivacyPolicyUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred while trying to fetch the privacy policy", ex);
                }
            });

            ProcessDeletionRequests().Add(this.watcher);
        }).Add(watcher).IsVital();

        while (!loadDatabase.task.IsCompleted || !logInToDiscord.task.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.task.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the database.", loadDatabase.task.Exception);
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.task.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the discord client.", logInToDiscord.task.Exception);
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.FailedDiscordLogin);
        }

        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            ExitApplication(true).Wait();
        };

        Console.CancelKeyPress += delegate
        {
            _logger.LogInfo("Exiting, please wait..");
            ExitApplication().Wait();
        };


        Task.Run(async () =>
        {
            while (true)
            {
                if (File.Exists("updated"))
                {
                    File.Delete("updated");
                    await ExitApplication();
                    return;
                }

                await Task.Delay(1000);
            }
        }).Add(watcher).IsVital();

        await Task.Delay(-1);
    }

    internal Task<int> GetPrefix(DiscordMessage message)
    {
        return Task<int>.Run(() =>
        {
            //if (IsDev)
            //    if (!_status.TeamMembers.Any(x => x == message.Author.Id))
            //        return -1;

            string currentPrefix = this.guilds.TryGetValue(message.GuildId ?? 0, out var guild) ? guild.PrefixSettings.Prefix : this.Prefix;

            int CommandStart = -1;

            if (!(guild?.PrefixSettings.PrefixDisabled ?? false))
                CommandStart = CommandsNextUtilities.GetStringPrefixLength(message, currentPrefix);

            if (CommandStart == -1)
                CommandStart = CommandsNextUtilities.GetMentionPrefixLength(message, this.discordClient.CurrentUser);

            return CommandStart;
        });
    }
    bool ExitCalled = false;

    internal async Task ExitApplication(bool Immediate = false)
    {
        _ = Task.Delay(Immediate ? TimeSpan.FromSeconds(10) : TimeSpan.FromMinutes(5)).ContinueWith(x =>
        {
            if (x.IsCompletedSuccessfully)
                Environment.Exit((int)ExitCodes.ExitTasksTimeout);
        });

        if (DatabaseClient.IsDisposed() || this.ExitCalled) // When the Database Client has been disposed, the Exit Call has already been made.
            return;

        this.ExitCalled = true;

        _logger.LogInfo("Preparing to shut down Makoto..");

        foreach (var b in this.Plugins)
        {
            _logger.LogInfo("Shutting down '{0}'..", b.Value.Name);
            await b.Value.Shutdown();
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

                await Util.Initializers.SyncTasks.ExecuteSyncTasks(this, this.discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            try
            {
                _logger.LogInfo("Closing Discord Client..");

                await this.discordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
                await this.discordClient.DisconnectAsync();

                _logger.LogDebug("Closed Discord Client.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close Discord Client gracefully.", ex);
            }
        }

        if (this.status.DatabaseInitialized)
        {
            try
            {
                _logger.LogInfo("Flushing to database..");
                await this.databaseClient.FullSyncDatabase(true);
                _logger.LogDebug("Flushed to database.");

                Thread.Sleep(500);

                _logger.LogInfo("Closing database..");
                await DatabaseClient.Dispose();
                _logger.LogDebug("Closed database.");
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Failed to close Database Client gracefully.", ex);
            }
        }

        Thread.Sleep(500);
        _logger.LogInfo("Goodbye!");

        Thread.Sleep(500);
        Environment.Exit(0);
    }

    private async Task ProcessDeletionRequests()
    {
        new Task(new Action(async () =>
        {
            ProcessDeletionRequests().Add(this.watcher);
        })).CreateScheduledTask(DateTime.UtcNow.AddHours(24));

        lock (this.users)
        {
            foreach (var b in this.users)
            {
                if ((b.Value?.Data?.DeletionRequested ?? false) && b.Value?.Data?.DeletionRequestDate.GetTimespanUntil() < TimeSpan.Zero)
                {
                    _logger.LogInfo("Deleting profile of '{Key}'", b.Key);

                    this.users.Remove(b.Key);
                    this.databaseClient._helper.DeleteRow(this.databaseClient.mainDatabaseConnection, "users", "userid", $"{b.Key}").Add(this.watcher);
                    this.objectedUsers.Add(b.Key);
                    foreach (var c in this.discordClient.Guilds.Where(x => x.Value.OwnerId == b.Key))
                    {
                        try
                        { _logger.LogInfo("Leaving guild '{guild}'..", c.Key); c.Value.LeaveAsync().Add(this.watcher); }
                        catch { }
                    }
                }
            }
        }
    }

    internal Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        => Util.Initializers.SyncTasks.GuildDownloadCompleted(this, sender, e);

    internal void LogHandler(object? sender, LogMessageEventArgs e)
        => TaskWatcher.LogHandler(this, sender, e);
}