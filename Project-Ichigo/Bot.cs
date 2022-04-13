namespace Project_Ichigo;

internal class Bot
{
    internal DiscordClient discordClient;
    internal LavalinkNodeConnection LavalinkNodeConnection;

    internal static DatabaseClient DatabaseClient { get; set; }
    internal DatabaseClient _databaseClient { get; set; }

    internal CollectionUpdates _collectionUpdates { get; set; }

    internal Status _status = new();
    internal ServerInfo _guilds = new();
    internal Users _users = new();

    internal PhishingUrlUpdater _phishingUrlUpdater { get; set; }

    internal PhishingUrls _phishingUrls = new();
    internal SubmissionBans _submissionBans = new();
    internal SubmittedUrls _submittedUrls = new();

    internal GlobalBans _globalBans = new();

    internal ScoreSaberClient _scoreSaberClient { get; set; }
    internal CountryCodes _countryCodes { get; set; }

    internal BumpReminder.BumpReminder _bumpReminder { get; set; }
    internal ExperienceHandler _experienceHandler { get; set; }

    internal TaskWatcher.TaskWatcher _watcher = new();

    internal ILogger _logger { get; set; }
    internal ILoggerProvider _loggerProvider { get; set; }

    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        _logger = StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LoggerObjects.LogLevel.INFO, DateTime.UtcNow.AddDays(-3), false);
        _loggerProvider = new Xorog.Logger.LoggerProvider();

        LogRaised += LogHandler;

        _collectionUpdates = new(this);

        LogInfo("Starting up..");

        try
        {
            if (args.Contains("--debug"))
            {
                ChangeLogLevel(LoggerObjects.LogLevel.DEBUG);
                LogInfo("Debug logs enabled");
            }
        }
        catch (Exception ex)
        {
            LogError($"An exception occured while to enable debug logs", ex);
        }

        _scoreSaberClient = ScoreSaberClient.InitializeScoresaber();

        LogDebug($"Enviroment Details\n\n" +
                $"Dotnet Version: {Environment.Version}\n" +
                $"OS & Version: {Environment.OSVersion}\n\n" +
                $"OS 64x: {Environment.Is64BitOperatingSystem}\n" +
                $"Process 64x: {Environment.Is64BitProcess}\n\n" +
                $"MachineName: {Environment.MachineName}\n" +
                $"UserName: {Environment.UserName}\n" +
                $"UserDomain: {Environment.UserDomainName}\n\n" +
                $"Current Directory: {Environment.CurrentDirectory}\n" +
                $"Commandline: {Regex.Replace(Environment.CommandLine, @"(--token \S*)", "")}\n");

        _bumpReminder = new(this);

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                Stopwatch databaseConnectionSc = new();
                databaseConnectionSc.Start();
                LogInfo($"Connecting to database..");

                DatabaseClient = await DatabaseClient.InitializeDatabase(this);
                _databaseClient = DatabaseClient;

                databaseConnectionSc.Stop();
                LogInfo($"Connected to database. ({databaseConnectionSc.ElapsedMilliseconds}ms)");
                _status.DatabaseInitialized = true;

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.UpdateCountryCodes();
                await _databaseInit.LoadValuesFromDatabase();
            }
            catch (Exception ex)
            {
                LogFatal($"An exception occured while initializing data", ex);
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLogin);
            }

            _phishingUrlUpdater = new(this);
            _ = _phishingUrlUpdater.UpdatePhishingUrlDatabase(_phishingUrls);
        });

        await loadDatabase.WaitAsync(TimeSpan.FromSeconds(30));

        bool IsDev = false;
        bool DevOnline = false;

        var logInToDiscord = Task.Run(async () =>
        {
            string token = "";

            try
            {
                if (args.Contains("--token"))
                    token = args[Array.IndexOf(args, "--token") + 1];
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to parse a token commandline argument", ex);
            }

            if (File.Exists("token.cfg") && !args.Contains("--token"))
                token = File.ReadAllText("token.cfg");

            if (!(token.Length > 0))
            {
                LogFatal("No token provided");
                File.WriteAllText("token.cfg", "");
                await Task.Delay(1000);
                Environment.Exit(ExitCodes.NoToken);
                return;
            }


            AddBlacklist(token);
            AddBlacklist(Secrets.Secrets.DatabasePassword);
            AddBlacklist(Secrets.Secrets.LavalinkPassword);


            LogDebug($"Registering DiscordClient..");

            var logger = new LoggerFactory();
            logger.AddProvider(_loggerProvider);

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = $"{token}",
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                Intents = DiscordIntents.All,
                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                AutoReconnect = true,
                LoggerFactory = logger
            });

            _experienceHandler = new(this);

            LogDebug($"Registering CommandsNext..");

            string Prefix = ";;";

            Task<int> GetPrefix(DiscordMessage message)
            {
                return Task<int>.Run(() =>
                {
                    if (!IsDev)
                        if (DevOnline)
                            if (_status.TeamMembers.Any(x => x == message.Author.Id))
                                if (message.Channel.GuildId is 929365338544545802 or 938490069839380510)
                                    return -1;

                    if (IsDev)
                        if (!_status.TeamMembers.Any(x => x == message.Author.Id))
                            return -1;

                    return CommandsNextUtilities.GetStringPrefixLength(message, Prefix);
                });
            }

            var cNext = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDefaultHelp = false,
                EnableMentionPrefix = false,
                IgnoreExtraArguments = true,
                EnableDms = false,
                ServiceProvider = new ServiceCollection()
                                .AddSingleton(this)
                                .BuildServiceProvider(),
                PrefixResolver = new PrefixResolverDelegate(GetPrefix)
            });



            LogDebug($"Registering Lavalink..");

            var endpoint = new ConnectionEndpoint
            {
                Hostname = Secrets.Secrets.LavalinkUrl,
                Port = Secrets.Secrets.LavalinkPort
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = Secrets.Secrets.LavalinkPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            discordClient.UseLavalink();



            LogDebug($"Registering Commands..");

            cNext.RegisterCommands<User>();
            cNext.RegisterCommands<Mod>();
            cNext.RegisterCommands<Admin>();

            cNext.RegisterCommands<Maintainers>();



            LogDebug($"Registering Command Converters..");

            cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());



            LogDebug($"Registering Command Events..");

            CommandEvents commandEvents = new(this);
            cNext.CommandExecuted += commandEvents.CommandExecuted;
            cNext.CommandErrored += commandEvents.CommandError;



            LogDebug($"Registering Afk Events..");

            AfkEvents afkEvents = new(this);
            discordClient.MessageCreated += afkEvents.MessageCreated;



            LogDebug($"Registering Crosspost Events..");

            CrosspostEvents crosspostEvents = new(this);
            discordClient.MessageCreated += crosspostEvents.MessageCreated;



            LogDebug($"Registering Phishing Events..");

            PhishingProtectionEvents phishingProtectionEvents = new(this);
            discordClient.MessageCreated += phishingProtectionEvents.MessageCreated;
            discordClient.MessageUpdated += phishingProtectionEvents.MessageUpdated;

            SubmissionEvents _submissionEvents = new(this);
            discordClient.ComponentInteractionCreated += _submissionEvents.ComponentInteractionCreated;



            LogDebug($"Registering Discord Events..");

            DiscordEvents discordEvents = new(this);
            discordClient.GuildCreated += discordEvents.GuildCreated;



            LogDebug($"Registering Actionlog Events..");

            ActionlogEvents actionlogEvents = new(this);

            discordClient.GuildMemberAdded += actionlogEvents.UserJoined;
            discordClient.GuildMemberRemoved += actionlogEvents.UserLeft;
            discordClient.MessageDeleted += actionlogEvents.MessageDeleted;
            discordClient.MessagesBulkDeleted += actionlogEvents.MessageBulkDeleted;
            discordClient.MessageUpdated += actionlogEvents.MessageUpdated;
            discordClient.GuildMemberUpdated += actionlogEvents.MemberUpdated;
            discordClient.GuildRoleCreated += actionlogEvents.RoleCreated;
            discordClient.GuildRoleDeleted += actionlogEvents.RoleDeleted;
            discordClient.GuildRoleUpdated += actionlogEvents.RoleModified;
            discordClient.GuildBanAdded += actionlogEvents.BanAdded;
            discordClient.GuildBanRemoved += actionlogEvents.BanRemoved;
            discordClient.GuildUpdated += actionlogEvents.GuildUpdated;
            discordClient.ChannelCreated += actionlogEvents.ChannelCreated;
            discordClient.ChannelDeleted += actionlogEvents.ChannelDeleted;
            discordClient.ChannelUpdated += actionlogEvents.ChannelUpdated;
            discordClient.InviteCreated += actionlogEvents.InviteCreated;
            discordClient.InviteDeleted += actionlogEvents.InviteDeleted;



            LogDebug($"Registering Join Events..");

            JoinEvents joinEvents = new(this);
            discordClient.GuildMemberAdded += joinEvents.GuildMemberAdded;
            discordClient.GuildMemberRemoved += joinEvents.GuildMemberRemoved;



            LogDebug($"Registering BumpReminder Events..");

            BumpReminderEvents bumpReminderEvents = new(this);
            discordClient.MessageCreated += bumpReminderEvents.MessageCreated;
            discordClient.MessageDeleted += bumpReminderEvents.MessageDeleted;
            discordClient.MessageReactionAdded += bumpReminderEvents.ReactionAdded;
            discordClient.MessageReactionRemoved += bumpReminderEvents.ReactionRemoved;



            LogDebug($"Registering Experience Events..");

            ExperienceEvents experienceEvents = new(this);
            discordClient.MessageCreated += experienceEvents.MessageCreated;



            LogDebug($"Registering Interactivity..");

            discordClient.UseInteractivity(new InteractivityConfiguration { });



            LogDebug($"Registering Events..");

            discordClient.GuildDownloadCompleted += GuildDownloadCompleted;



            try
            {
                var discordLoginSc = new Stopwatch();
                discordLoginSc.Start();

                _ = Task.Delay(10000).ContinueWith(t =>
                {
                    if (!_status.DiscordInitialized)
                    {
                        LogError($"An exception occured while trying to log into discord: The log in took longer than 5 seconds");
                        Environment.Exit(ExitCodes.FailedDiscordLogin);
                        return;
                    }
                });

                LogInfo("Connecting and authenticating with Discord..");
                await discordClient.ConnectAsync();

                discordLoginSc.Stop();
                LogInfo($"Connected and authenticated with Discord. ({discordLoginSc.ElapsedMilliseconds}ms)");
                _status.DiscordInitialized = true;

                IsDev = (discordClient.CurrentApplication.Id == 929373806437470260);

                if (!IsDev)
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var bot = await discordClient.GetUserAsync(929373806437470260, true);

                                if (bot.Presence is null)
                                {
                                    DevOnline = false;
                                    await Task.Delay(8000);
                                    continue;
                                }

                                bool isOnline = (bot.Presence.ClientStatus.Web.Value == UserStatus.Online);

                                if (isOnline != DevOnline)
                                {
                                    DevOnline = isOnline;
                                    LogWarn($"Developer client status changed: {isOnline}");
                                }
                            }
                            catch (Exception ex)
                            {
                                LogError($"An exception occured while trying to request the status of the developer client", ex);
                            }
                            await Task.Delay(8000);
                        }
                    }).Add(_watcher);

                _ = Task.Run(() =>
                {
                    try
                    {
                        _status.TeamMembers.AddRange(discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                        LogInfo($"Added {_status.TeamMembers.Count} users to administrator list");
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to log into discord", ex);
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDiscordLogin);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var lavalinkSc = new Stopwatch();
                    lavalinkSc.Start();
                    LogInfo("Connecting and authenticating with Lavalink..");

                    LavalinkNodeConnection = await discordClient.GetLavalink().ConnectAsync(lavalinkConfig);
                    lavalinkSc.Stop();
                    LogInfo($"Connected and authenticated with Lavalink. ({lavalinkSc.ElapsedMilliseconds}ms)");

                    _status.LavalinkInitialized = true;
                }
                catch (Exception ex)
                {
                    LogError($"An exception occured while trying to log into Lavalink", ex);
                    return;
                }
            });
        });

        while (!loadDatabase.IsCompleted || !logInToDiscord.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.IsCompletedSuccessfully)
        {
            LogFatal($"An uncaught exception occured while initializing the database.");
            Environment.Exit(ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.IsCompletedSuccessfully)
        {
            LogFatal($"An uncaught exception occured while initializing the discord client.");
            Environment.Exit(ExitCodes.FailedDiscordLogin);
        }

        _ = DatabaseClient.QueueWatcher();
        _watcher.Watcher();

        AppDomain.CurrentDomain.ProcessExit += async delegate
        {
            await RunExitTasks(null, null);
        };

        //Console.CancelKeyPress += async delegate
        //{
        //    LogInfo("Exiting, please wait..");
        //    await FlushToDatabase(null, null);
        //};


        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (File.Exists("updated"))
                {
                    File.Delete("updated");
                    await RunExitTasks(null, null);
                    Environment.Exit(0);
                    return;
                }

                await Task.Delay(1000);
            }
        });

        _ = Task.Run(async () =>
        {
            await Task.Delay(10000);

            while (true)
            {
                try
                {
                    if (_databaseClient.IsDisposed())
                        return;

                    await discordClient.UpdateStatusAsync(userStatus: (DevOnline ? UserStatus.DoNotDisturb : UserStatus.Online), activity: new DiscordActivity($"{discordClient.Guilds.Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} guilds | Up for {Math.Round((DateTime.UtcNow - _status.startupTime).TotalHours, 2).ToString(CultureInfo.CreateSpecificCulture("en-US"))}h | {_status.DebugRaised}D {_status.InfoRaised}I {_status.WarnRaised}W {_status.ErrorRaised}E {_status.FatalRaised}F", ActivityType.Playing));
                    await Task.Delay(30000);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to update user status", ex);
                    await Task.Delay(30000);
                }
            }
        });

        await Task.Delay(-1);
    }

    private void LogHandler(object? sender, LogMessageEventArgs e)
    {
        switch (e.LogEntry.LogLevel)
        {
            case LoggerObjects.LogLevel.DEBUG2:
            case LoggerObjects.LogLevel.DEBUG:
            {
                _status.DebugRaised++;
                break;
            }
            case LoggerObjects.LogLevel.INFO:
            {
                _status.InfoRaised++;
                break;
            }
            case LoggerObjects.LogLevel.WARN:
            {
                _status.WarnRaised++;
                break;
            }
            case LoggerObjects.LogLevel.ERROR:
            {
                _status.ErrorRaised++;
                break;
            }
            case LoggerObjects.LogLevel.FATAL:
            {
                if (e.LogEntry.Message.ToLower().Contains("'not authenticated.'"))
                {
                    LogRaised -= LogHandler;
                    _ = RunExitTasks(null, null);
                }

                _status.FatalRaised++;
                break;
            }
        }
    }

    private async Task FlushToDatabase()
    {
        LogInfo($"Flushing to database..");
        await DatabaseClient.SyncDatabase(true);
        LogDebug($"Flushed to database.");

        await Task.Delay(1000);

        LogInfo($"Closing database..");
        await DatabaseClient.Dispose();
        LogDebug($"Closed database.");
    }

    private async Task LogOffDiscord()
    {
        // Apparently spamming status changes makes it more consistent. Dont ask me how.
        // It's important that the status changes before the bot shuts down to work around a library issue.

        LogInfo($"Closing Discord Client..");

        for (int i = 0; i < 10; i++)
            await discordClient.UpdateStatusAsync(userStatus: UserStatus.Idle);
        await Task.Delay(1000);
        for (int i = 0; i < 10; i++)
            await discordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
        await Task.Delay(5000);
        await discordClient.DisconnectAsync();
        LogDebug($"Closed Discord Client.");
    }

    private async Task RunExitTasks(object? sender, EventArgs e)
    {
        if (DatabaseClient.IsDisposed())
            return;

        await LogOffDiscord();
        await FlushToDatabase();

        Thread.Sleep(1000);
        LogInfo($"Goodbye!");
    }

    private async Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Run(async () =>
        {
            LogInfo($"I'm on {e.Guilds.Count} guilds.");

            LogDebug($"{string.Join(", ", e.Guilds.Select(x => x.Value.Name))}");

            for (int i = 0; i < 251; i++)
            {
                _experienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!_guilds.Servers.ContainsKey(guild.Key))
                    _guilds.Servers.Add(guild.Key, new ServerInfo.ServerSettings());

                if (_guilds.Servers[guild.Key].BumpReminderSettings.Enabled)
                {
                    _bumpReminder.ScheduleBump(sender, guild.Key);
                }

                foreach (var member in guild.Value.Members)
                {
                    if (!_guilds.Servers[guild.Key].Members.ContainsKey(member.Value.Id))
                    {
                        _guilds.Servers[guild.Key].Members.Add(member.Value.Id, new());
                    }

                    _experienceHandler.CheckExperience(member.Key, guild.Value);
                }
            }

            await DatabaseClient.CheckGuildTables();
            await DatabaseClient.SyncDatabase(true);

        }).Add(_watcher);
    }
}