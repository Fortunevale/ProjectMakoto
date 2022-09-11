namespace ProjectIchigo;

public class Bot
{
    #region Clients

    internal static DatabaseClient DatabaseClient { get; set; }

    internal DiscordClient discordClient;
    internal LavalinkNodeConnection LavalinkNodeConnection;

    internal DatabaseClient databaseClient { get; set; }
    internal ScoreSaberClient scoreSaberClient { get; set; }
    internal TranslationClient translationClient { get; set; }

    #endregion Clients


    #region Util

    internal CountryCodes countryCodes { get; set; }
    internal LanguageCodes languageCodes { get; set; }

    internal BumpReminder bumpReminder { get; set; }
    internal ExperienceHandler experienceHandler { get; set; }
    internal TaskWatcher watcher = new();
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

    #endregion Bans


    internal Status status = new();
    internal Dictionary<ulong, Guild> guilds = new();
    internal Dictionary<ulong, User> users = new();

    internal string Prefix { get; private set; } = ";;";


    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        _logger = StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.INFO, DateTime.UtcNow.AddDays(-3), false);
        var loggerProvider = _logger._provider;

        _logger.LogRaised += LogHandler;

        try
        {
            string ASCII = File.ReadAllText("Assets/ASCII.txt");
            Console.WriteLine();
            foreach (var b in ASCII)
            {
                switch (b)
                {
                    case 'g':
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case 'r':
                        Console.ForegroundColor = ConsoleColor.Red;
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
            _logger.LogError($"Failed to render ASCII art", ex);
        }

        Console.ResetColor();

        string RunningVersion = (File.Exists("LatestGitPush.cfg") ? File.ReadLines("LatestGitPush.cfg") : new List<string> { "Development-Build" }).ToList()[0].Trim();

        _logger.LogInfo($"Starting up Ichigo {RunningVersion}..\n");

        try
        {
            if (args.Contains("--debug"))
            {
                _logger.ChangeLogLevel(LogLevel.TRACE);
                _logger.LogInfo("Debug logs enabled");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while to enable debug logs", ex);
        }

        scoreSaberClient = ScoreSaberClient.InitializeScoresaber();
        translationClient = TranslationClient.Initialize();

        _logger.LogDebug($"Environment Details\n\n" +
                $"Dotnet Version: {Environment.Version}\n" +
                $"OS & Version: {Environment.OSVersion}\n\n" +
                $"OS 64x: {Environment.Is64BitOperatingSystem}\n" +
                $"Process 64x: {Environment.Is64BitProcess}\n\n" +
                $"MachineName: {Environment.MachineName}\n" +
                $"UserName: {Environment.UserName}\n" +
                $"UserDomain: {Environment.UserDomainName}\n\n" +
                $"Current Directory: {Environment.CurrentDirectory}\n" +
                $"Commandline: {Regex.Replace(Environment.CommandLine, @"(--token \S*)", "")}\n");

        bumpReminder = new(this);

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                _logger.LogDebug($"Loading config..");

                if (!File.Exists("config.json"))
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Include }));

                status.LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                File.WriteAllText("config.json", JsonConvert.SerializeObject(status.LoadedConfig, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Include }));
                _logger.LogInfo($"Config loaded.");

                Task.Run(async () =>
                {
                    DateTime lastModify = new();

                    while (true)
                    {
                        try
                        {
                            FileInfo fileInfo = new("config.json");

                            if (lastModify != fileInfo.LastWriteTimeUtc)
                            {
                                try
                                {
                                    _logger.LogDebug($"Reloading config..");
                                    status.LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                                    _logger.LogInfo($"Config reloaded.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to reload config", ex);
                                }
                            }

                            lastModify = fileInfo.LastWriteTimeUtc;

                            await Task.Delay(1000);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("An exception occurred while trying to reload the config.json", ex);
                            await Task.Delay(10000);
                        }
                    }
                }).Add(watcher);

                _logger.LogInfo($"Connecting to database..");

                DatabaseClient = await DatabaseClient.InitializeDatabase(this);
                databaseClient = DatabaseClient;

                _logger.LogInfo($"Connected to database.");
                status.DatabaseInitialized = true;

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.UpdateCountryCodes();
                await _databaseInit.LoadValuesFromDatabase();

                _ = Task.Run(async () =>
                {
                    _logger.LogDebug("Waiting for guilds to download to sync database..");

                    while (!discordClient.Guilds.Any())
                        Thread.Sleep(500);

                    await databaseClient.FullSyncDatabase(true);

                    _logger.LogInfo("Initial Full Sync finished.");
                    status.DatabaseInitialLoadCompleted = true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogFatal($"An exception occurred while initializing data", ex);
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLogin);
            }

            _ = new PhishingUrlUpdater(this).UpdatePhishingUrlDatabase();
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
                _logger.LogError($"An exception occurred while trying to parse a token commandline argument", ex);
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
            _logger.AddBlacklist(status.LoadedConfig.Secrets.Database.Password);
            _logger.AddBlacklist(status.LoadedConfig.Secrets.Lavalink.Password);
            _logger.AddBlacklist(status.LoadedConfig.Secrets.Github.Token);
            _logger.AddBlacklist(status.LoadedConfig.Secrets.KawaiiRedToken);

            _logger.AddLogLevelBlacklist(LogLevel.TRACE2);

            _logger.LogDebug($"Registering LoggerFactory..");

            var logger = new LoggerFactory();
            logger.AddProvider(loggerProvider);

            _logger.LogDebug($"Registering DiscordClient..");

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = $"{token}",
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace,
                Intents = DiscordIntents.All,
                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                AutoReconnect = true,
                LoggerFactory = logger,
                HttpTimeout = TimeSpan.FromSeconds(60),
                MessageCacheSize = 4096,
            });

            experienceHandler = new(this);

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
                Hostname = status.LoadedConfig.Secrets.Lavalink.Host,
                Port = status.LoadedConfig.Secrets.Lavalink.Port
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = status.LoadedConfig.Secrets.Lavalink.Password,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            discordClient.UseLavalink();



            _logger.LogDebug($"Registering Commands..");

            cNext.RegisterCommands<PrefixCommands.UtilityPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.MusicPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.SocialPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ScoreSaberPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ModerationPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ConfigurationPrefixCommands>();
            
            cNext.RegisterCommands<PrefixCommands.MaintainersPrefixCommands>();



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
            discordClient.UserUpdated += discordEventHandler.UserUpdated;



            _logger.LogDebug($"Registering Interactivity..");

            discordClient.UseInteractivity(new InteractivityConfiguration { });



            _logger.LogDebug($"Registering Events..");

            discordClient.GuildDownloadCompleted += GuildDownloadCompleted;



            try
            {
                _ = Task.Delay(10000).ContinueWith(t =>
                {
                    if (!status.DiscordInitialized)
                    {
                        _logger.LogError($"An exception occurred while trying to log into discord: The log in took longer than 10 seconds");
                        Environment.Exit(ExitCodes.FailedDiscordLogin);
                        return;
                    }
                });

                var appCommands = discordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
                {
                    ServiceProvider = new ServiceCollection()
                                        .AddSingleton(this)
                                        .BuildServiceProvider(),
                    EnableDefaultHelp = false
                });

                if (!status.LoadedConfig.IsDev)
                {
                    appCommands.RegisterGlobalCommands<ApplicationCommands.MaintainersAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ConfigurationAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ModerationAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.SocialAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ScoreSaberAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.MusicAppCommands>();
                    appCommands.RegisterGlobalCommands<ApplicationCommands.UtilityAppCommands>();
                }
                else
                {
                    appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(status.LoadedConfig.Channels.Assets);
                    appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(status.LoadedConfig.Channels.Assets);
                }

                _logger.LogInfo("Connecting and authenticating with Discord..");
                await discordClient.ConnectAsync();

                _logger.LogInfo($"Connected and authenticated with Discord.");
                status.DiscordInitialized = true;

                if (status.LoadedConfig.IsDev)
                    Prefix = ">>";

                _ = Task.Run(async () =>
                {
                    if (status.LoadedConfig.DontModify.LastStartedVersion != RunningVersion)
                    {
                        status.LoadedConfig.DontModify.LastStartedVersion = RunningVersion;
                        File.WriteAllText("config.json", JsonConvert.SerializeObject(status.LoadedConfig, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Include }));

                        var channel = await discordClient.GetChannelAsync(status.LoadedConfig.Channels.GithubLog);
                        await channel.SendMessageAsync(new DiscordEmbedBuilder
                        {
                            Color = EmbedColors.Success,
                            Title = $"Successfully updated to `{RunningVersion}`."
                        }); 
                    }
                });

                _ = Task.Run(() =>
                {
                    try
                    {
                        status.TeamMembers.AddRange(discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                        _logger.LogInfo($"Added {status.TeamMembers.Count} users to administrator list");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception occurred while trying to log into discord", ex);
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDiscordLogin);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    if (status.LoadedConfig.Lavalink.UseAutoUpdater)
                    {
                        try
                        {
                            string v = status.LoadedConfig.Lavalink.JarFolderPath.Replace("\\", "/");

                            if (v.EndsWith("/"))
                                v = v[..^1];

                            string VersionFile = $"{v}/Lavalink.ver";
                            string JarFile = $"{v}/Lavalink.jar";
                            string PidFile = $"{v}/Lavalink.pid";

                            string LatestVersion = "";
                            string InstalledVersion = "";

                            if (File.Exists(VersionFile))
                            {
                                InstalledVersion = File.ReadAllText(VersionFile);
                            }

                            var client = new GitHubClient(new ProductHeaderValue("Project-Ichigo"));

                            var releases = await client.Repository.Release.GetAll("freyacodes", "Lavalink");

                            Release workingRelease = null;

                            foreach (var b in releases)
                            {
                                if (b.Prerelease)
                                {
                                    if (status.LoadedConfig.Lavalink.DownloadPreRelease)
                                    {
                                        workingRelease = b;
                                        break;
                                    }
                                    else
                                        continue;
                                }

                                workingRelease = b;
                                break;
                            }

                            if (workingRelease is null)
                                throw new Exception();

                            LatestVersion = workingRelease.TagName;

                            if (LatestVersion != InstalledVersion)
                            {
                                _logger.LogInfo($"Lavalink is not up to date. Updating from {InstalledVersion} to {LatestVersion}..");

                                try
                                {
                                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                                    {
                                        _logger.LogInfo($"Running on windows, killing Lavalink before updating if it exists..");

                                        if (File.Exists(PidFile))
                                            Process.GetProcessById(Convert.ToInt32(File.ReadAllText(PidFile))).Kill();
                                    }
                                }
                                catch { }

                                if (File.Exists($"{JarFile}.old"))
                                    File.Delete($"{JarFile}.old");

                                if (File.Exists(JarFile))
                                    File.Move(JarFile, $"{JarFile}.old");

                                HttpClient http = new();

                                var response = await http.GetStreamAsync(workingRelease.Assets.First(x => x.Name.EndsWith(".jar")).BrowserDownloadUrl);

                                using (var io = new FileStream(JarFile, System.IO.FileMode.CreateNew))
                                {
                                    await response.CopyToAsync(io);
                                }

                                File.WriteAllText(VersionFile, LatestVersion);

                                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                                {
                                    _logger.LogInfo($"Lavalink updated to {LatestVersion}. Killing old Lavalink Process if it exists..");

                                    if (File.Exists(PidFile))
                                        Process.GetProcessById(Convert.ToInt32(File.ReadAllText(PidFile))).Kill();
                                }

                                _logger.LogDebug($"Waiting for Lavalink to start back up..");
                                await Task.Delay(60000);
                            }
                            else
                            {
                                _logger.LogInfo("Lavalink is up to date!");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Failed to update Lavalink", ex);
                        }
                    }

                    _logger.LogInfo("Connecting and authenticating with Lavalink..");
                    LavalinkNodeConnection = await discordClient.GetLavalink().ConnectAsync(lavalinkConfig);
                    _logger.LogInfo($"Connected and authenticated with Lavalink.");

                    status.LavalinkInitialized = true;

                    try
                    {
                        _logger.LogInfo($"Lavalink is running on {await LavalinkNodeConnection.Rest.GetVersionAsync()}.");
                    } catch { }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An exception occurred while trying to log into Lavalink", ex);
                    return;
                }
            });
        });

        while (!loadDatabase.IsCompleted || !logInToDiscord.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.IsCompletedSuccessfully)
        {
            _logger.LogFatal($"An uncaught exception occurred while initializing the database.", loadDatabase.Exception);
            await Task.Delay(1000);
            Environment.Exit(ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.IsCompletedSuccessfully)
        {
            _logger.LogFatal($"An uncaught exception occurred while initializing the discord client.", logInToDiscord.Exception);
            await Task.Delay(1000);
            Environment.Exit(ExitCodes.FailedDiscordLogin);
        }

        _ = DatabaseClient.QueueWatcher();
        watcher.Watcher();

        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            ExitApplication().Wait();
        };

        Console.CancelKeyPress += delegate
        {
            _logger.LogInfo("Exiting, please wait..");
            ExitApplication().Wait();
        };


        _ = Task.Run(async () =>
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
        });

        _ = Task.Run(async () =>
        {
            Thread.Sleep(5000);

            _ = discordClient.UpdateStatusAsync(userStatus: UserStatus.Online, activity: new DiscordActivity("Registering commands..", ActivityType.Playing));

            while (discordClient.GetApplicationCommands().RegisteredCommands.Count == 0)
                Thread.Sleep(1000);

            _ = discordClient.UpdateStatusAsync(userStatus: UserStatus.Online, activity: new DiscordActivity("Commands registered. Bot is available again!", ActivityType.Playing));
            Thread.Sleep(10000);

            status.DiscordCommandsRegistered = true;

            while (true)
            {
                try
                {
                    if (databaseClient.IsDisposed())
                        return;

                    List<ulong> users = new();

                    foreach (var b in guilds)
                        foreach (var c in b.Value.Members)
                            if (!users.Contains(c.Key))
                                users.Add(c.Key);

                    foreach (var b in this.users)
                        if (!users.Contains(b.Key))
                            users.Add(b.Key);

                    await discordClient.UpdateStatusAsync(activity: new DiscordActivity($"{discordClient.Guilds.Count.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))} guilds | Up for {Math.Round((DateTime.UtcNow - status.startupTime).TotalHours, 2).ToString(CultureInfo.CreateSpecificCulture("en-US"))}h", ActivityType.Playing));
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
                        b.Add(watcher);
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

                if (objectedUsers.Contains(guild.Value.OwnerId) || bannedUsers.ContainsKey(guild.Value.OwnerId) || bannedGuilds.ContainsKey(guild.Key))
                {
                    _logger.LogInfo($"Leaving guild '{guild.Key}'..");
                    await guild.Value.LeaveAsync();
                    return;
                }

                var guildMembers = await guild.Value.GetAllMembersAsync();
                var guildBans = await guild.Value.GetBansAsync();

                foreach (var member in guildMembers)
                {
                    if (!guilds[guild.Key].Members.ContainsKey(member.Id))
                        guilds[guild.Key].Members.Add(member.Id, new(guilds[guild.Key], member.Id));

                    if (guilds[guild.Key].Members[member.Id].FirstJoinDate == DateTime.UnixEpoch)
                        guilds[guild.Key].Members[member.Id].FirstJoinDate = member.JoinedAt.UtcDateTime;

                    if (guilds[guild.Key].Members[member.Id].LastLeaveDate != DateTime.UnixEpoch)
                        guilds[guild.Key].Members[member.Id].LastLeaveDate = DateTime.UnixEpoch;

                    guilds[guild.Key].Members[member.Id].MemberRoles = member.Roles.Select(x => new MemberRole
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToList();

                    guilds[guild.Key].Members[member.Id].SavedNickname = member.Nickname;
                }

                foreach (var databaseMember in guilds[guild.Key].Members.ToList())
                {
                    if (!guildMembers.Any(x => x.Id == databaseMember.Key))
                    {
                        if (guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate == DateTime.UnixEpoch)
                            guilds[guild.Key].Members[databaseMember.Key].LastLeaveDate = DateTime.UtcNow;
                    }
                }

                foreach (var banEntry in guildBans)
                {
                    if (!guilds[guild.Key].Members.ContainsKey(banEntry.User.Id))
                        continue;

                    if (guilds[guild.Key].Members[banEntry.User.Id].MemberRoles.Count > 0)
                        guilds[guild.Key].Members[banEntry.User.Id].MemberRoles.Clear();

                    if (guilds[guild.Key].Members[banEntry.User.Id].SavedNickname != "")
                        guilds[guild.Key].Members[banEntry.User.Id].SavedNickname = "";
                }

                if (guilds[guild.Key].InviteTrackerSettings.Enabled)
                {
                    await InviteTrackerEvents.UpdateCachedInvites(this, guild.Value);
                }

                startupTasksSuccess++;
            }));
        }

        foreach (var guild in Guilds)
        {
            try
            {
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

                    _logger.LogDebug($"Requesting more threads for '{guild.Key}'");
                }

                foreach (var b in Threads.Where(x => x.CurrentMember is null))
                {
                    _logger.LogDebug($"Joining thread on '{guild.Key}': {b.Id}");
                    await b.JoinAsync();
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to join threads on '{guild.Key}'", ex);
            }
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

            Task.Run(async () =>
            {
                while (!status.LavalinkInitialized)
                    await Task.Delay(1000);

                Dictionary<string, TimeSpan> VideoLengthCache = new();

                foreach (var user in users)
                {
                    foreach (var list in user.Value.UserPlaylists)
                    {
                        foreach (var b in list.List.ToList())
                        {
                            if (b.Length is null || !b.Length.HasValue)
                            {
                                if (!VideoLengthCache.ContainsKey(b.Url))
                                {
                                    _logger.LogInfo($"Fetching video length for '{b.Url}'");

                                    var track = await discordClient.GetLavalink().ConnectedNodes.First(x => x.Value.IsConnected).Value.Rest.GetTracksAsync(b.Url, LavalinkSearchType.Plain);

                                    if (track.LoadResultType != LavalinkLoadResultType.TrackLoaded)
                                    {
                                        list.List.Remove(b);
                                        _logger.LogError($"Failed to load video length for '{b.Url}'");
                                        continue;
                                    }

                                    VideoLengthCache.Add(b.Url, track.Tracks.First().Length);
                                    await Task.Delay(100);
                                }

                                b.Length = VideoLengthCache[b.Url];
                            }
                        }
                    }
                }
            }).Add(watcher);

            for (int i = 0; i < 251; i++)
            {
                experienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!guilds.ContainsKey(guild.Key))
                    guilds.Add(guild.Key, new Guild(guild.Key));

                if (guilds[guild.Key].BumpReminderSettings.Enabled)
                {
                    bumpReminder.ScheduleBump(sender, guild.Key);
                }

                foreach (var member in guild.Value.Members)
                {
                    if (!guilds[guild.Key].Members.ContainsKey(member.Value.Id))
                    {
                        guilds[guild.Key].Members.Add(member.Value.Id, new(guilds[guild.Key], member.Value.Id));
                    }

                    experienceHandler.CheckExperience(member.Key, guild.Value);
                }

                if (guilds[guild.Key].CrosspostSettings.CrosspostChannels.Any())
                {
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < guilds[guild.Key].CrosspostSettings.CrosspostChannels.Count; i++)
                        {
                            if (guild.Value is null)
                                return;

                            var ChannelId = guilds[guild.Key].CrosspostSettings.CrosspostChannels[i];

                            _logger.LogDebug($"Checking channel '{ChannelId}' for missing crossposts..");

                            if (!guild.Value.Channels.ContainsKey(ChannelId))
                                return;

                            var Messages = await guild.Value.GetChannel(ChannelId).GetMessagesAsync(20);

                            if (Messages.Any(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                foreach (var msg in Messages.Where(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                {
                                    _logger.LogDebug($"Handling missing crosspost message '{msg.Id}' in '{msg.ChannelId}' for '{guild.Key}'..");

                                    var WaitTime = guilds[guild.Value.Id].CrosspostSettings.DelayBeforePosting - msg.Id.GetSnowflakeTime().GetTotalSecondsSince();

                                    if (WaitTime > 0)
                                        await Task.Delay(TimeSpan.FromSeconds(WaitTime));

                                    if (guilds[guild.Value.Id].CrosspostSettings.DelayBeforePosting > 3)
                                        _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode("🕒"));

                                    bool ReactionAdded = false;

                                    var task = guilds[guild.Value.Id].CrosspostSettings.CrosspostWithRatelimit(msg.Channel, msg).ContinueWith(s =>
                                    {
                                        if (ReactionAdded)
                                            _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                    });

                                    await Task.Delay(5000);

                                    if (!task.IsCompleted)
                                    {
                                        await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                                        ReactionAdded = true;
                                    }

                                    while (!task.IsCompleted)
                                        task.Wait();
                                }
                        }
                    }).Add(watcher);
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

            await databaseClient.CheckGuildTables();
            await databaseClient.FullSyncDatabase(true);

            List<DiscordUser> UserCache = new();

            await Task.Delay(5000);

            while (!status.LavalinkInitialized)
                await Task.Delay(1000);

            foreach (var guild in e.Guilds)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (guilds[guild.Key].Lavalink.ChannelId != 0)
                        {
                            if (!guild.Value.Channels.ContainsKey(guilds[guild.Key].Lavalink.ChannelId))
                                throw new Exception("Channel no longer exists");

                            if (guilds[guild.Key].Lavalink.CurrentVideo.ToLower().Contains("localhost") || guilds[guild.Key].Lavalink.CurrentVideo.ToLower().Contains("127.0.0.1"))
                                throw new Exception("Localhost?");

                            if (guilds[guild.Key].Lavalink.SongQueue.Count > 0)
                            {
                                for (var i = 0; i < guilds[guild.Key].Lavalink.SongQueue.Count; i++)
                                {
                                    Lavalink.QueueInfo b = guilds[guild.Key].Lavalink.SongQueue[i];

                                    _logger.LogDebug($"Fixing queue info for {b.Url}");

                                    b.guild = guild.Value;

                                    if (!UserCache.Any(x => x.Id == b.UserId))
                                    {
                                        _logger.LogDebug($"Fetching user '{b.UserId}'");
                                        UserCache.Add(await discordClient.GetUserAsync(b.UserId));
                                    }

                                    b.user = UserCache.First(x => x.Id == b.UserId);
                                }
                            }

                            var channel = guild.Value.GetChannel(guilds[guild.Key].Lavalink.ChannelId);

                            var lava = discordClient.GetLavalink();
                            var node = lava.ConnectedNodes.Values.First();
                            var conn = node.GetGuildConnection(guild.Value);

                            if (conn is null)
                            {
                                if (!lava.ConnectedNodes.Any())
                                {
                                    throw new Exception("Lavalink connection isn't established.");
                                }

                                conn = await node.ConnectAsync(channel);
                            }

                            var loadResult = await node.Rest.GetTracksAsync(guilds[guild.Key].Lavalink.CurrentVideo, LavalinkSearchType.Plain);

                            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                                return;

                            await conn.PlayAsync(loadResult.Tracks.First());

                            await Task.Delay(2000);
                            await conn.SeekAsync(TimeSpan.FromSeconds(guilds[guild.Key].Lavalink.CurrentVideoPosition));

                            guilds[guild.Key].Lavalink.QueueHandler(this, discordClient, node, conn);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to continue music playback for '{guild.Key}'", ex);
                        guilds[guild.Key].Lavalink = new(guilds[guild.Key]);
                    }
                });

                await Task.Delay(1000);
            }
        }).Add(watcher);
    }

    bool ExitCalled = false;

    internal async Task ExitApplication(bool Immediate = false)
    {
        _ = Task.Delay(Immediate ? TimeSpan.FromSeconds(10) : TimeSpan.FromMinutes(5)).ContinueWith(x =>
        {
            if (x.IsCompletedSuccessfully)
                Environment.Exit(ExitCodes.ExitTasksTimeout);
        });

        if (DatabaseClient.IsDisposed() || ExitCalled) // When the Database Client has been disposed, the Exit Call has already been made.
            return;

        ExitCalled = true;

        _logger.LogInfo($"Preparing to shut down Ichigo..");

        if (status.DiscordInitialized)
        {
            try
            {
                Stopwatch sw = new();
                sw.Start();

                if (!status.DiscordCommandsRegistered)
                    _logger.LogWarn("Startup is incomplete. Waiting for Startup to finish to shutdown..");

                while (!status.DiscordCommandsRegistered && sw.ElapsedMilliseconds < TimeSpan.FromMinutes(5).TotalMilliseconds)
                    await Task.Delay(500);

                await SyncTasks(discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            try
            {
                _logger.LogInfo($"Closing Discord Client..");

                await discordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
                await discordClient.DisconnectAsync();

                _logger.LogDebug($"Closed Discord Client.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close Discord Client gracefully.", ex);
            }
        }

        if (status.DatabaseInitialized)
        {
            try
            {
                _logger.LogInfo($"Flushing to database..");
                await DatabaseClient.FullSyncDatabase(true);
                _logger.LogDebug($"Flushed to database.");

                Thread.Sleep(500);

                _logger.LogInfo($"Closing database..");
                await DatabaseClient.Dispose();
                _logger.LogDebug($"Closed database.");
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Failed to close Database Client gracefully.", ex);
            }
        }

        Thread.Sleep(500);
        _logger.LogInfo($"Goodbye!");

        Thread.Sleep(500);
        Environment.Exit(0);
    }

    private async void LogHandler(object? sender, LogMessageEventArgs e)
    {
        switch (e.LogEntry.LogLevel)
        {
            case LogLevel.FATAL:
            case LogLevel.ERROR:
            {
                if (status.DiscordInitialized)
                {
                    if (e.LogEntry.Message is "[111] Connection terminated (4000, ''), reconnecting" or "[111] Connection terminated (-1, ''), reconnecting")
                        break;

                    var channel = discordClient.Guilds[status.LoadedConfig.Channels.Assets].GetChannel(status.LoadedConfig.Channels.ExceptionLog);

                    _ = channel.SendMessageAsync(new DiscordEmbedBuilder()
                        .WithColor(e.LogEntry.LogLevel == LogLevel.FATAL ? new DiscordColor("#FF0000") : EmbedColors.Error)
                        .WithTitle(e.LogEntry.LogLevel.GetName().ToLower().FirstLetterToUpper())
                        .WithDescription($"```\n{e.LogEntry.Message.SanitizeForCodeBlock()}\n```{(e.LogEntry.Exception is not null ? $"\n```cs\n{e.LogEntry.Exception.ToString().SanitizeForCodeBlock()}```" : "")}")
                        .WithTimestamp(e.LogEntry.TimeOfEvent)); 
                }
                break;
            }
        }

        switch (e.LogEntry.LogLevel)
        {
            case LogLevel.FATAL:
            {
                if (e.LogEntry.Message.ToLower().Contains("'not authenticated.'"))
                {
                    status.DiscordDisconnections++;

                    if (status.DiscordDisconnections >= 3)
                    {
                        _logger.LogRaised -= LogHandler;
                        _ = ExitApplication();
                    }
                    else
                    {
                        try
                        {
                            await discordClient.ConnectAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogFatal("Failed to reconnect to discord", ex);
                            _logger.LogRaised -= LogHandler;
                            _ = ExitApplication();
                        }
                    }
                }
                else if (e.LogEntry.Message.ToLower().Contains("open DataReader associated".ToLower()))
                {
                    status.DataReaderExceptions++;

                    if (status.DataReaderExceptions >= 4)
                    {
                        _logger.LogFatal("4 or more DataReader Exceptions triggered, exiting..");

                        _logger.LogRaised -= LogHandler;
                        _ = ExitApplication();
                    }
                }
                break;
            }
            default:
                break;
        }
    }
}