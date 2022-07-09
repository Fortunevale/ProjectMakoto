using User = ProjectIchigo.Commands.User.User;

namespace ProjectIchigo;

internal class Bot
{
    internal static DatabaseClient DatabaseClient { get; set; }

    internal DiscordClient discordClient;
    internal LavalinkNodeConnection LavalinkNodeConnection;


    internal DatabaseClient _databaseClient { get; set; }


    internal Status _status = new();
    internal Guilds _guilds = new();
    internal Users _users = new();


    internal PhishingUrlUpdater _phishingUrlUpdater { get; set; }

    internal PhishingUrls _phishingUrls = new();
    internal PhishingSubmissionBans _submissionBans = new();
    internal SubmittedUrls _submittedUrls = new();


    internal GlobalBans _globalBans = new();


    internal ScoreSaberClient _scoreSaberClient { get; set; }
    internal CountryCodes _countryCodes { get; set; }


    internal BumpReminder _bumpReminder { get; set; }
    internal ExperienceHandler _experienceHandler { get; set; }


    internal TaskWatcher _watcher = new();

    internal ILogger _ilogger { get; set; }
    internal ILoggerProvider _loggerProvider { get; set; }

    internal string Prefix { get; private set; } = ";;";
    internal bool IsDev { get; private set; } = false;


    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        _logger = StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.INFO, DateTime.UtcNow.AddDays(-3), false);
        _loggerProvider = _logger._provider;

        _logger.LogRaised += LogHandler;

        _logger.LogInfo("Starting up..");

        try
        {
            if (args.Contains("--debug"))
            {
                _logger.ChangeLogLevel(LogLevel.DEBUG);
                _logger.LogInfo("Debug logs enabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occured while to enable debug logs", ex);
        }

        _scoreSaberClient = ScoreSaberClient.InitializeScoresaber();

        _logger.LogDebug($"Enviroment Details\n\n" +
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
                _logger.LogInfo($"Connecting to database..");

                DatabaseClient = await DatabaseClient.InitializeDatabase(this);
                _databaseClient = DatabaseClient;

                databaseConnectionSc.Stop();
                _logger.LogInfo($"Connected to database. ({databaseConnectionSc.ElapsedMilliseconds}ms)");
                _status.DatabaseInitialized = true;

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.UpdateCountryCodes();
                await _databaseInit.LoadValuesFromDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogFatal($"An exception occured while initializing data", ex);
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLogin);
            }

            _phishingUrlUpdater = new(this);
            _ = _phishingUrlUpdater.UpdatePhishingUrlDatabase(_phishingUrls);
        });

        await loadDatabase.WaitAsync(TimeSpan.FromSeconds(600));

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
                _logger.LogError($"An exception occured while trying to parse a token commandline argument", ex);
            }

            if (File.Exists("token.cfg") && !args.Contains("--token"))
                token = File.ReadAllText("token.cfg");

            if (!(token.Length > 0))
            {
                _logger.LogFatal("No token provided");
                File.WriteAllText("token.cfg", "");
                await Task.Delay(1000);
                Environment.Exit(ExitCodes.NoToken);
                return;
            }


            _logger.AddBlacklist(token);
            _logger.AddBlacklist(Secrets.Secrets.DatabasePassword);
            _logger.AddBlacklist(Secrets.Secrets.LavalinkPassword);

            _logger.AddLogLevelBlacklist(LogLevel.TRACE2);


            _logger.LogDebug($"Registering DiscordClient..");

            var logger = new LoggerFactory();
            logger.AddProvider(_loggerProvider);

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = $"{token}",
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace,
                Intents = DiscordIntents.All,
                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                AutoReconnect = true,
                LoggerFactory = logger
            });

            _experienceHandler = new(this);

            _logger.LogDebug($"Registering CommandsNext..");

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



            _logger.LogDebug($"Registering Lavalink..");

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



            _logger.LogDebug($"Registering Commands..");

            cNext.RegisterCommands<User>();
            cNext.RegisterCommands<Music>();
            cNext.RegisterCommands<Social>();
            cNext.RegisterCommands<Commands.User.ScoreSaber>();
            cNext.RegisterCommands<Mod>();
            cNext.RegisterCommands<Admin>();
            
            cNext.RegisterCommands<Commands.Maintainers.Maintainers>();



            _logger.LogDebug($"Registering Command Converters..");

            cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());



            _logger.LogDebug($"Registering DisCatSharp EventHandler..");

            DiscordEventHandler discordEventHandler = new(this);

            discordClient.GuildCreated += discordEventHandler.GuildCreated;
            discordClient.GuildUpdated += discordEventHandler.GuildUpdated;

            discordClient.ChannelCreated += discordEventHandler.ChannelCreated;
            discordClient.ChannelDeleted += discordEventHandler.ChannelDeleted;
            discordClient.ChannelUpdated += discordEventHandler.ChannelUpdated;

            discordClient.GuildMemberAdded += discordEventHandler.GuildMemberAdded;
            discordClient.GuildMemberRemoved += discordEventHandler.GuildMemberRemoved;
            discordClient.GuildMemberUpdated += discordEventHandler.GuildMemberUpdated;
            discordClient.GuildBanAdded += discordEventHandler.GuildBanAdded;
            discordClient.GuildBanRemoved += discordEventHandler.GuildBanRemoved;

            discordClient.InviteCreated += discordEventHandler.InviteCreated;
            discordClient.InviteDeleted += discordEventHandler.InviteDeleted;

            cNext.CommandExecuted += discordEventHandler.CommandExecuted;
            cNext.CommandErrored += discordEventHandler.CommandError;

            discordClient.MessageCreated += discordEventHandler.MessageCreated;
            discordClient.MessageDeleted += discordEventHandler.MessageDeleted;
            discordClient.MessagesBulkDeleted += discordEventHandler.MessagesBulkDeleted;
            discordClient.MessageUpdated += discordEventHandler.MessageUpdated;

            discordClient.MessageReactionAdded += discordEventHandler.MessageReactionAdded;
            discordClient.MessageReactionRemoved += discordEventHandler.MessageReactionRemoved;

            discordClient.ComponentInteractionCreated += discordEventHandler.ComponentInteractionCreated;

            discordClient.GuildRoleCreated += discordEventHandler.GuildRoleCreated;
            discordClient.GuildRoleDeleted += discordEventHandler.GuildRoleDeleted;
            discordClient.GuildRoleUpdated += discordEventHandler.GuildRoleUpdated;

            discordClient.VoiceStateUpdated += discordEventHandler.VoiceStateUpdated;

            discordClient.ThreadCreated += discordEventHandler.ThreadCreated;
            discordClient.ThreadDeleted += discordEventHandler.ThreadDeleted;
            discordClient.ThreadMemberUpdated += discordEventHandler.ThreadMemberUpdated;
            discordClient.ThreadMembersUpdated += discordEventHandler.ThreadMembersUpdated;
            discordClient.ThreadUpdated += discordEventHandler.ThreadUpdated;
            discordClient.ThreadListSynced += discordEventHandler.ThreadListSynced;



            _logger.LogDebug($"Registering Interactivity..");

            discordClient.UseInteractivity(new InteractivityConfiguration { });



            _logger.LogDebug($"Registering Events..");

            discordClient.GuildDownloadCompleted += GuildDownloadCompleted;



            try
            {
                var discordLoginSc = new Stopwatch();
                discordLoginSc.Start();

                _ = Task.Delay(10000).ContinueWith(t =>
                {
                    if (!_status.DiscordInitialized)
                    {
                        _logger.LogError($"An exception occured while trying to log into discord: The log in took longer than 10 seconds");
                        Environment.Exit(ExitCodes.FailedDiscordLogin);
                        return;
                    }
                });

                _logger.LogInfo("Connecting and authenticating with Discord..");
                await discordClient.ConnectAsync();

                discordLoginSc.Stop();
                _logger.LogInfo($"Connected and authenticated with Discord. ({discordLoginSc.ElapsedMilliseconds}ms)");
                _status.DiscordInitialized = true;

                IsDev = (discordClient.CurrentApplication.Id == 929373806437470260);

                Task.Run(async () =>
                {
                    var appCommands = discordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
                    {
                        ServiceProvider = new ServiceCollection()
                                .AddSingleton(this)
                                .BuildServiceProvider(),
                        EnableDefaultHelp = false
                    });

                    if (IsDev)
                        appCommands.RegisterGuildCommands<ApplicationCommands.Maintainers.Maintainers>(929365338544545802);
                    else
                        appCommands.RegisterGlobalCommands<ApplicationCommands.Maintainers.Maintainers>();
                }).Add(_watcher);

                if (IsDev)
                    Prefix = ">>";

                _ = Task.Run(() =>
                {
                    try
                    {
                        _status.TeamMembers.AddRange(discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                        _logger.LogInfo($"Added {_status.TeamMembers.Count} users to administrator list");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occured trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception occured while trying to log into discord", ex);
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
                    _logger.LogInfo("Connecting and authenticating with Lavalink..");

                    LavalinkNodeConnection = await discordClient.GetLavalink().ConnectAsync(lavalinkConfig);
                    lavalinkSc.Stop();
                    _logger.LogInfo($"Connected and authenticated with Lavalink. ({lavalinkSc.ElapsedMilliseconds}ms)");

                    _status.LavalinkInitialized = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An exception occured while trying to log into Lavalink", ex);
                    return;
                }
            });
        });

        while (!loadDatabase.IsCompleted || !logInToDiscord.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.IsCompletedSuccessfully)
        {
            _logger.LogFatal($"An uncaught exception occured while initializing the database.", loadDatabase.Exception);
            await Task.Delay(1000);
            Environment.Exit(ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.IsCompletedSuccessfully)
        {
            _logger.LogFatal($"An uncaught exception occured while initializing the discord client.", logInToDiscord.Exception);
            await Task.Delay(1000);
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
        //    _logger.LogInfo("Exiting, please wait..");
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

                    List<ulong> users = new();

                    foreach (var b in _guilds.List)
                        foreach (var c in b.Value.Members)
                            if (!users.Contains(c.Key))
                                users.Add(c.Key);

                    foreach (var b in _users.List)
                        if (!users.Contains(b.Key))
                            users.Add(b.Key);

                    await discordClient.UpdateStatusAsync(userStatus: UserStatus.Online, activity: new DiscordActivity($"{discordClient.Guilds.Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} guilds | Serving {users.Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} users | Up for {Math.Round((DateTime.UtcNow - _status.startupTime).TotalHours, 2).ToString(CultureInfo.CreateSpecificCulture("en-US"))}h | {_status.WarnRaised}W {_status.ErrorRaised}E {_status.FatalRaised}F", ActivityType.Playing));
                    await Task.Delay(30000);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to update user status", ex);
                    await Task.Delay(30000);
                }
            }
        });

        await Task.Delay(-1);
    }

    internal Task<int> GetPrefix(DiscordMessage message)
    {
        return Task<int>.Run(() =>
        {
            //if (IsDev)
            //    if (!_status.TeamMembers.Any(x => x == message.Author.Id))
            //        return -1;

            var CommandStart = CommandsNextUtilities.GetStringPrefixLength(message, Prefix);

            if (CommandStart == -1 && (message.Content.StartsWith($"<@{discordClient.CurrentUser.Id}>") || message.Content.StartsWith($"<@!{discordClient.CurrentUser.Id}>")))
                CommandStart = CommandsNextUtilities.GetMentionPrefixLength(message, discordClient.CurrentUser);

            return CommandStart;
        });
    }

    private async Task SyncTasks(IReadOnlyDictionary<ulong, DiscordGuild> Guilds)
    {
        ObservableCollection<Task> runningTasks = new();

        NotifyCollectionChangedEventHandler runningTasksUpdated()
        {
            return (s, e) =>
            {
                if (e is not null && e.NewItems is not null)
                    foreach (Task b in e.NewItems)
                    {
                        _logger.LogDebug($"Adding sync task to watcher: {b.Id}");
                        b.Add(_watcher);
                    }
            };
        }

        runningTasks.CollectionChanged += runningTasksUpdated();

        int startupTasksSuccess = 0;

        foreach (var guild in Guilds)
        {
            while (runningTasks.Count >= 4 && !runningTasks.Any(x => x.IsCompleted))
                await Task.Delay(100);

            foreach (var task in runningTasks.ToList())
                if (task.IsCompleted)
                    runningTasks.Remove(task);

            runningTasks.Add(Task.Run(async () =>
            {
                _logger.LogDebug($"Performing sync tasks for '{guild.Key}'..");
                var guildMembers = await guild.Value.GetAllMembersAsync();
                var guildBans = await guild.Value.GetBansAsync();

                foreach (var member in guildMembers)
                {
                    if (!_guilds.List[guild.Key].Members.ContainsKey(member.Id))
                        _guilds.List[guild.Key].Members.Add(member.Id, new());

                    if (_guilds.List[guild.Key].Members[member.Id].FirstJoinDate == DateTime.UnixEpoch)
                        _guilds.List[guild.Key].Members[member.Id].FirstJoinDate = member.JoinedAt.UtcDateTime;

                    if (_guilds.List[guild.Key].Members[member.Id].LastLeaveDate != DateTime.UnixEpoch)
                        _guilds.List[guild.Key].Members[member.Id].LastLeaveDate = DateTime.UnixEpoch;

                    _guilds.List[guild.Key].Members[member.Id].MemberRoles = member.Roles.Select(x => new MemberRole
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToList();

                    _guilds.List[guild.Key].Members[member.Id].SavedNickname = member.Nickname;
                }

                foreach (var databaseMember in _guilds.List[guild.Key].Members.ToList())
                {
                    if (!guildMembers.Any(x => x.Id == databaseMember.Key))
                    {
                        if (_guilds.List[guild.Key].Members[databaseMember.Key].LastLeaveDate == DateTime.UnixEpoch)
                            _guilds.List[guild.Key].Members[databaseMember.Key].LastLeaveDate = DateTime.UtcNow;
                    }
                }

                foreach (var banEntry in guildBans)
                {
                    if (!_guilds.List[guild.Key].Members.ContainsKey(banEntry.User.Id))
                        continue;

                    if (_guilds.List[guild.Key].Members[banEntry.User.Id].MemberRoles.Count > 0)
                        _guilds.List[guild.Key].Members[banEntry.User.Id].MemberRoles.Clear();

                    if (_guilds.List[guild.Key].Members[banEntry.User.Id].SavedNickname != "")
                        _guilds.List[guild.Key].Members[banEntry.User.Id].SavedNickname = "";
                }

                if (_guilds.List[guild.Key].InviteTrackerSettings.Enabled)
                {
                    await InviteTrackerEvents.UpdateCachedInvites(this, guild.Value);
                }

                List<DiscordThreadChannel> Threads = new();

                while (true)
                {
                    var t = await guild.Value.GetActiveThreadsAsync();

                    foreach (var b in t.ReturnedThreads.Values)
                    {
                        if (!Threads.Contains(b) && b is not null)
                            Threads.Add(b);
                    }

                    if (!t.HasMore)
                        break;
                }

                foreach (var b in Threads)
                {
                    _logger.LogDebug(JsonConvert.SerializeObject(b));
                    _ = b.JoinAsync();
                }

                startupTasksSuccess++;
            }));
        }

        while (runningTasks.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        runningTasks.CollectionChanged -= runningTasksUpdated();
        runningTasks.Clear();

        _logger.LogInfo($"Sync Tasks successfully finished for {startupTasksSuccess}/{Guilds.Count} guilds.");
    }

    private async Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Run(async () =>
        {
            _logger.LogInfo($"I'm on {e.Guilds.Count} guilds.");

            for (int i = 0; i < 251; i++)
            {
                _experienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!_guilds.List.ContainsKey(guild.Key))
                    _guilds.List.Add(guild.Key, new Guilds.ServerSettings());

                if (_guilds.List[guild.Key].BumpReminderSettings.Enabled)
                {
                    _bumpReminder.ScheduleBump(sender, guild.Key);
                }

                foreach (var member in guild.Value.Members)
                {
                    if (!_guilds.List[guild.Key].Members.ContainsKey(member.Value.Id))
                    {
                        _guilds.List[guild.Key].Members.Add(member.Value.Id, new());
                    }

                    _experienceHandler.CheckExperience(member.Key, guild.Value);
                }

                if (_guilds.List[guild.Key].CrosspostSettings.CrosspostTasks.Any())
                {
                    Task.Run(async () =>
                    {
                        for (var i = 0; i < _guilds.List[guild.Key].CrosspostSettings.CrosspostTasks.Count; i++)
                        {
                            CrosspostMessage b = _guilds.List[guild.Key].CrosspostSettings.CrosspostTasks[0];

                            if (!guild.Value?.Channels.ContainsKey(b.ChannelId) ?? true)
                                return;

                            var channel = guild.Value.GetChannel(b.ChannelId);

                            if (!channel.TryGetMessage(b.MessageId, out var msg))
                            {
                                if (_guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Any(x => x.MessageId == b.MessageId))
                                {
                                    var obj = _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.First(x => x.MessageId == b.MessageId);
                                    _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Remove(obj);
                                }
                                continue;
                            }

                            _logger.LogDebug($"Handling missing crosspost message '{b.MessageId}' in '{b.ChannelId}' for '{guild.Key}'..");

                            var WaitTime = _guilds.List[guild.Value.Id].CrosspostSettings.DelayBeforePosting - b.MessageId.GetSnowflakeTime().GetTotalSecondsSince();

                            if (WaitTime > 0)
                                await Task.Delay(TimeSpan.FromSeconds(WaitTime));

                            if (_guilds.List[guild.Value.Id].CrosspostSettings.DelayBeforePosting > 3)
                                _ = msg.DeleteOwnReactionAsync(DiscordEmoji.FromUnicode("🕒"));

                            bool ReactionAdded = false;

                            try
                            {
                                var task = channel.CrosspostMessageAsync(msg).ContinueWith(s =>
                                {
                                    if (_guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Any(x => x.MessageId == b.MessageId))
                                    {
                                        var obj = _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.First(x => x.MessageId == b.MessageId);
                                        _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Remove(obj);
                                    }

                                    if (ReactionAdded)
                                        _ = msg.DeleteOwnReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                });

                                await Task.Delay(5000);

                                if (!task.IsCompleted)
                                {
                                    await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                    ReactionAdded = true;
                                }
                            }
                            catch (ArgumentException)
                            {
                                if (_guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Any(x => x.MessageId == b.MessageId))
                                {
                                    var obj = _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.First(x => x.MessageId == b.MessageId);
                                    _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Remove(obj);
                                }
                                return;
                            }
                            catch (AggregateException ex)
                            {
                                if (ex.InnerException is ArgumentException aex)
                                {
                                    if (_guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Any(x => x.MessageId == b.MessageId))
                                    {
                                        var obj = _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.First(x => x.MessageId == b.MessageId);
                                        _guilds.List[guild.Value.Id].CrosspostSettings.CrosspostTasks.Remove(obj);
                                    }
                                    return;
                                }

                                throw;
                            }
                        }
                    }).Add(_watcher);
                }
            }

            try
            {
                await SyncTasks(discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            await _databaseClient.CheckGuildTables();
            await _databaseClient.SyncDatabase(true);

        }).Add(_watcher);
    }

    private async Task FlushToDatabase()
    {
        _logger.LogInfo($"Flushing to database..");
        await DatabaseClient.SyncDatabase(true);
        _logger.LogDebug($"Flushed to database.");

        await Task.Delay(1000);

        _logger.LogInfo($"Closing database..");
        await DatabaseClient.Dispose();
        _logger.LogDebug($"Closed database.");
    }

    private async Task LogOffDiscord()
    {
        // Apparently spamming status changes makes it more consistent. Dont ask me how.
        // It's important that the status changes before the bot shuts down to work around a library issue.

        _logger.LogInfo($"Closing Discord Client..");

        for (int i = 0; i < 10; i++)
            await discordClient.UpdateStatusAsync(userStatus: UserStatus.Idle);
        await Task.Delay(1000);
        for (int i = 0; i < 10; i++)
            await discordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
        await Task.Delay(5000);
        await discordClient.DisconnectAsync();
        _logger.LogDebug($"Closed Discord Client.");
    }

    private async Task RunExitTasks(object? sender, EventArgs e)
    {
        if (DatabaseClient.IsDisposed())
            return;

        try
        {
            await SyncTasks(discordClient.Guilds);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to run sync tasks", ex);
        }

        await LogOffDiscord();
        await FlushToDatabase();

        Thread.Sleep(1000);
        _logger.LogInfo($"Goodbye!");
    }

    private void LogHandler(object? sender, LogMessageEventArgs e)
    {
        switch (e.LogEntry.LogLevel)
        {
            case LogLevel.WARN:
            {
                _status.WarnRaised++;
                break;
            }
            case LogLevel.ERROR:
            {
                _status.ErrorRaised++;
                break;
            }
            case LogLevel.FATAL:
            {
                if (e.LogEntry.Message.ToLower().Contains("'not authenticated.'"))
                {
                    _logger.LogRaised -= LogHandler;
                    _ = RunExitTasks(null, null);
                }
                else if (e.LogEntry.Message.ToLower().Contains("open DataReader associated".ToLower()))
                {
                    _status.DataReaderExceptions++;

                    if (_status.DataReaderExceptions >= 4)
                    {
                        _logger.LogFatal("4 or more DataReader Exceptions triggered, exiting..");

                        _logger.LogRaised -= LogHandler;
                        _ = RunExitTasks(null, null);
                    }
                }

                _status.FatalRaised++;
                break;
            }
            default:
                break;
        }
    }
}