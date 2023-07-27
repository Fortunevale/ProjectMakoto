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

    public DatabaseClient DatabaseClient { get; set; }

    public DiscordClient DiscordClient { get; internal set; }
    internal LavalinkSession LavalinkSession;

    internal ScoreSaberClient ScoreSaberClient { get; set; }
    public GoogleTranslateClient TranslationClient { get; internal set; }
    public ThreadJoinClient ThreadJoinClient { get; internal set; }
    public AbuseIpDbClient AbuseIpDbClient { get; internal set; }
    public MonitorClient MonitorClient { get; internal set; }
    internal GitHubClient GithubClient { get; set; }

    #endregion Clients

    #region Plugins
    public IReadOnlyDictionary<string, BasePlugin> Plugins
    => this._Plugins.AsReadOnly();
    internal Dictionary<string, BasePlugin> _Plugins { get; set; } = new();

    public IReadOnlyDictionary<string, List<BasePluginCommand>> PluginCommands
        => this._PluginCommands.AsReadOnly();
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
    internal Dictionary<string, PhishingUrlEntry> PhishingHosts = new();
    internal Dictionary<ulong, SubmittedUrlEntry> SubmittedHosts = new();

    #endregion Util


    #region Bans

    internal List<ulong> objectedUsers = new();
    internal Dictionary<ulong, BanDetails> bannedUsers = new();
    internal Dictionary<ulong, BanDetails> bannedGuilds = new();

    internal Dictionary<ulong, BanDetails> globalBans = new();
    internal Dictionary<ulong, List<BanDetails>> globalNotes = new();

    #endregion Bans


    public Status status = new();
    internal SelfFillingDictionary<Entities.Guild> Guilds = null;
    internal SelfFillingDictionary<Entities.User> Users = null;

    internal string RawFetchedPrivacyPolicy = "";
    internal string Prefix { get; private set; } = ";;";

    internal async Task Init(string[] args)
    {
        _logger = LoggerClient.StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", CustomLogLevel.Info, DateTime.UtcNow.AddDays(-3), false);
        _logger.LogRaised += LogHandler;

        UniversalExtensions.AttachLogger(_logger);

        RenderAsciiArt();

        this.status.RunningVersion = (File.Exists("LatestGitPush.cfg") ? File.ReadLines("LatestGitPush.cfg") : new List<string> { "Development-Build" }).ToList()[0].Trim();
        _logger.LogInfo("Starting up Makoto {RunningVersion}..\n", this.status.RunningVersion);

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

                this.Users = new(this);
                this.Guilds = new(this);
                await DatabaseClient.InitializeDatabase(this);

                this.BumpReminder = new(this);

                this.ScoreSaberClient = new ScoreSaberClient();
                this.TranslationClient = new GoogleTranslateClient();
                this.ThreadJoinClient = new ThreadJoinClient();

                await Util.Initializers.ListLoader.Load(this);
                await Util.Initializers.TranslationLoader.Load(this);
                await Util.Initializers.PluginLoader.LoadPlugins(this);

                this.MonitorClient = new MonitorClient(this);
                this.AbuseIpDbClient = new AbuseIpDbClient(this);

                this.GithubClient = new GitHubClient(new ProductHeaderValue("ProjectMakoto", this.status.RunningVersion));
                this.GithubClient.Credentials = new Credentials(this.status.LoadedConfig.Secrets.Github.Token);

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.LoadValuesFromDatabase();
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
                    Environment.Exit((int)ExitCodes.FailedDiscordLogin);
                    return;
                }
            });

            await Util.Initializers.DisCatSharpExtensionsLoader.Load(this, args);

            _logger.LogInfo("Connecting and authenticating with Discord..");
            await this.DiscordClient.ConnectAsync();
            _logger.LogInfo("Connected and authenticated with Discord.");

            this.status.DiscordInitialized = true;

            _ = Task.Run(async () =>
            {
                if (this.status.LoadedConfig.DontModify.LastStartedVersion == this.status.RunningVersion)
                    return;

                this.status.LoadedConfig.DontModify.LastStartedVersion = this.status.RunningVersion;
                this.status.LoadedConfig.Save();

                var channel = await this.DiscordClient.GetChannelAsync(this.status.LoadedConfig.Channels.GithubLog);
                await channel.SendMessageAsync(new DiscordEmbedBuilder
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

            ProcessDeletionRequests().Add(this);
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
        }).Add(this).IsVital();

        await Task.Delay(-1);
    }

    private static void RenderAsciiArt()
    {
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
    }

    internal Task<int> GetPrefix(DiscordMessage message)
    {
        return Task<int>.Run(() =>
        {
            //if (IsDev)
            //    if (!_status.TeamMembers.Any(x => x == message.Author.Id))
            //        return -1;

            string currentPrefix = this.Guilds.TryGetValue(message.GuildId ?? 0, out var guild) ? guild.PrefixSettings.Prefix : this.Prefix;

            int CommandStart = -1;

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
        _ = Task.Delay(Immediate ? TimeSpan.FromSeconds(10) : TimeSpan.FromMinutes(5)).ContinueWith(x =>
        {
            if (x.IsCompletedSuccessfully)
                Environment.Exit((int)ExitCodes.ExitTasksTimeout);
        });

        if (this.DatabaseClient.IsDisposed() || this.ExitCalled) // When the Database Client has been disposed, the Exit Call has already been made.
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

                await Util.Initializers.SyncTasks.ExecuteSyncTasks(this, this.DiscordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            try
            {
                _logger.LogInfo("Closing Discord Client..");

                await this.DiscordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
                await this.DiscordClient.DisconnectAsync();

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
                await this.DatabaseClient.FullSyncDatabase(true);
                _logger.LogDebug("Flushed to database.");

                Thread.Sleep(500);

                _logger.LogInfo("Closing database..");
                await this.DatabaseClient.Dispose();
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
            ProcessDeletionRequests().Add(this);
        })).CreateScheduledTask(DateTime.UtcNow.AddHours(24));

        lock (this.Users)
        {
            foreach (var b in this.Users)
            {
                if ((b.Value?.Data?.DeletionRequested ?? false) && b.Value?.Data?.DeletionRequestDate.GetTimespanUntil() < TimeSpan.Zero)
                {
                    _logger.LogInfo("Deleting profile of '{Key}'", b.Key);

                    this.Users.Remove(b.Key);
                    this.DatabaseClient._helper.DeleteRow(this.DatabaseClient.mainDatabaseConnection, "users", "userid", $"{b.Key}").Add(this);
                    this.objectedUsers.Add(b.Key);
                    foreach (var c in this.DiscordClient.Guilds.Where(x => x.Value.OwnerId == b.Key))
                    {
                        try
                        { _logger.LogInfo("Leaving guild '{guild}'..", c.Key); c.Value.LeaveAsync().Add(this); }
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