using ProjectIchigo.PrefixCommands;
using System.Reflection;

namespace ProjectIchigo;

public class Bot
{
    #region Clients

    internal static DatabaseClient DatabaseClient { get; set; }

    internal DiscordClient discordClient;
    internal LavalinkNodeConnection LavalinkNodeConnection;

    internal DatabaseClient databaseClient { get; set; }
    internal ScoreSaberClient scoreSaberClient { get; set; }
    internal GoogleTranslateClient translationClient { get; set; }
    internal ThreadJoinClient threadJoinClient { get; set; }
    internal AbuseIpDbClient abuseIpDbClient { get; set; }

    #endregion Clients


    #region Util

    internal static Translations loadedTranslations { get; set; }

    internal CountryCodes countryCodes { get; set; }
    internal LanguageCodes languageCodes { get; set; }
    internal IReadOnlyList<string> profanityList { get; set; }

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
    internal Dictionary<ulong, List<GlobalBanDetails>> globalNotes = new();

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
            _logger.LogError("Failed to render ASCII art", ex);
        }

        Console.ResetColor();

        string RunningVersion = (File.Exists("LatestGitPush.cfg") ? File.ReadLines("LatestGitPush.cfg") : new List<string> { "Development-Build" }).ToList()[0].Trim();

        _logger.LogInfo("Starting up Ichigo {RunningVersion}..\n", RunningVersion);

        if (args.Contains("--debug"))
        {
            _logger.ChangeLogLevel(LogLevel.DEBUG);
        }

        _logger.LogDebug("Loading all assemblies..");

        var assemblyCount = 0;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            LoadReferencedAssembly(assembly);
        }

        void LoadReferencedAssembly(Assembly assembly)
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    assemblyCount++;
                    _logger.LogDebug("Loading {Name}..", name.Name);
                    LoadReferencedAssembly(Assembly.Load(name));
                }
            }
        }

        _logger.LogInfo("Loaded {assemblyCount} assemblies.", assemblyCount);

        scoreSaberClient = ScoreSaberClient.InitializeScoresaber();
        translationClient = GoogleTranslateClient.Initialize();
        threadJoinClient = ThreadJoinClient.Initialize();

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

        bumpReminder = new(this);

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                countryCodes = new();
                List<string[]> cc = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText("Assets/Countries.json"));
                foreach (var b in cc)
                {
                    countryCodes.List.Add(b[2], new CountryCodes.CountryInfo
                    {
                        Name = b[0],
                        ContinentCode = b[1],
                        ContinentName = b[1].ToLower() switch
                        {
                            "af" => "Africa",
                            "an" => "Antarctica",
                            "as" => "Asia",
                            "eu" => "Europe",
                            "na" => "North America",
                            "oc" => "Oceania",
                            "sa" => "South America",
                            _ => "Invalid Continent"
                        }
                    });
                }

                _logger.LogDebug("Loaded {Count} countries", countryCodes.List.Count);


                languageCodes = new();
                List<string[]> lc = JsonConvert.DeserializeObject<List<string[]>>(File.ReadAllText("Assets/Languages.json"));
                foreach (var b in lc)
                {
                    languageCodes.List.Add(new LanguageCodes.LanguageInfo
                    {
                        Code = b[0],
                        Name = b[1],
                    });
                }
                _logger.LogDebug("Loaded {Count} languages", languageCodes.List.Count);

                profanityList = JsonConvert.DeserializeObject<List<string>>(await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/zacanger/profane-words/master/words.json"));
                _logger.LogDebug("Loaded {Count} profanity words", profanityList.Count);

                if (!File.Exists("config.json"))
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Include }));

                loadedTranslations = JsonConvert.DeserializeObject<Translations>(File.ReadAllText("Translations/strings.json"), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
                _logger.LogDebug("Loaded Translations");

                Task.Run(async () =>
                {
                    DateTime lastModify = new();

                    status.LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                    await Task.Delay(500);
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(status.LoadedConfig, Formatting.Indented));

                    while (true)
                    {
                        try
                        {
                            FileInfo fileInfo = new("config.json");

                            if (lastModify != fileInfo.LastWriteTimeUtc || status.LoadedConfig is null)
                            {
                                try
                                {
                                    _logger.LogDebug("Reloading config..");
                                    status.LoadedConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
                                    _logger.LogInfo("Config reloaded.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Failed to reload config", ex);
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
                await Task.Delay(1000);

                abuseIpDbClient = AbuseIpDbClient.Initialize(this);

                _logger.LogInfo("Connecting to database..");

                DatabaseClient = await DatabaseClient.InitializeDatabase(this);
                databaseClient = DatabaseClient;

                _logger.LogInfo("Connected to database.");
                status.DatabaseInitialized = true;

                DatabaseInit _databaseInit = new(this);

                await _databaseInit.LoadValuesFromDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogFatal("An exception occurred while initializing data", ex);
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
                _logger.LogError("An exception occurred while trying to parse a token commandline argument", ex);
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


            _logger.AddBlacklist(token, status.LoadedConfig.Secrets.Database.Password, status.LoadedConfig.Secrets.Lavalink.Password, status.LoadedConfig.Secrets.Github.Token, status.LoadedConfig.Secrets.KawaiiRedToken);

            _logger.AddLogLevelBlacklist(LogLevel.TRACE2);

            _logger.LogDebug("Registering LoggerFactory..");

            var logger = new LoggerFactory();
            logger.AddProvider(loggerProvider);

            _logger.LogDebug("Registering DiscordClient..");

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

            _logger.LogDebug("Registering CommandsNext..");

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



            _logger.LogDebug("Registering Lavalink..");

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



            _logger.LogDebug("Registering Commands..");

            cNext.RegisterCommands<PrefixCommands.UtilityPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.MusicPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.SocialPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ScoreSaberPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ModerationPrefixCommands>();
            cNext.RegisterCommands<PrefixCommands.ConfigurationPrefixCommands>();
            
            cNext.RegisterCommands<PrefixCommands.MaintainersPrefixCommands>();



            _logger.LogDebug("Registering Command Converters..");

            cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());



            _logger.LogDebug("Registering DisCatSharp EventHandler..");

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



            _logger.LogDebug("Registering Interactivity..");

            discordClient.UseInteractivity(new InteractivityConfiguration { });



            _logger.LogDebug("Registering Events..");

            discordClient.GuildDownloadCompleted += GuildDownloadCompleted;



            try
            {
                _ = Task.Delay(10000).ContinueWith(t =>
                {
                    if (!status.DiscordInitialized)
                    {
                        _logger.LogError("An exception occurred while trying to log into discord: {0}", "The log in took longer than 10 seconds");
                        Environment.Exit(ExitCodes.FailedDiscordLogin);
                        return;
                    }
                });

                var appCommands = discordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
                {
                    ServiceProvider = new ServiceCollection()
                                        .AddSingleton(this)
                                        .BuildServiceProvider(),
                    EnableDefaultHelp = false,
                    EnableLocalization = true
                });

                void GetCommandTranslations(ApplicationCommandsTranslationContext x)
                { 
                    x.AddSingleTranslation(File.ReadAllText("Translations/single_commands.json")); 
                    x.AddGroupTranslation(File.ReadAllText("Translations/group_commands.json")); 
                }

                if (!status.LoadedConfig.IsDev)
                {
                    appCommands.RegisterGlobalCommands<ApplicationCommands.MaintainersAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ConfigurationAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ModerationAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.SocialAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.ScoreSaberAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.MusicAppCommands>(GetCommandTranslations);
                    appCommands.RegisterGlobalCommands<ApplicationCommands.UtilityAppCommands>(GetCommandTranslations);
                }
                else
                {
                    appCommands.RegisterGuildCommands<ApplicationCommands.UtilityAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.MaintainersAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ConfigurationAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ModerationAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.SocialAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.ScoreSaberAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                    appCommands.RegisterGuildCommands<ApplicationCommands.MusicAppCommands>(status.LoadedConfig.Channels.Assets, GetCommandTranslations);
                }

                _logger.LogInfo("Connecting and authenticating with Discord..");
                await discordClient.ConnectAsync();

                _logger.LogInfo("Connected and authenticated with Discord.");
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
                        status.TeamOwner = discordClient.CurrentApplication.Team.Owner.Id;
                        _logger.LogInfo("Set {TeamOwner} as owner of the bot", status.TeamOwner);

                        status.TeamMembers.AddRange(discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                        _logger.LogInfo("Added {Count} users to administrator list", status.TeamMembers.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred trying to add team members to administrator list. Is the current bot registered in a team?", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while trying to log into discord", ex);
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
                                _logger.LogInfo("Lavalink is not up to date. Updating from {InstalledVersion} to {LatestVersion}..", InstalledVersion, LatestVersion);

                                try
                                {
                                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                                    {
                                        _logger.LogInfo("Running on windows, killing Lavalink before updating if it exists..");

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
                                    _logger.LogInfo("Lavalink updated to {LatestVersion}. Killing old Lavalink Process if it exists..", LatestVersion);

                                    if (File.Exists(PidFile))
                                        Process.GetProcessById(Convert.ToInt32(File.ReadAllText(PidFile))).Kill();
                                }

                                _logger.LogDebug("Waiting for Lavalink to start back up..");
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
                    _logger.LogInfo("Connected and authenticated with Lavalink.");

                    status.LavalinkInitialized = true;

                    try
                    {
                        _logger.LogInfo("Lavalink is running on {Version}.", await LavalinkNodeConnection.Rest.GetVersionAsync());
                    } catch { }
                }
                catch (Exception ex)
                {
                    _logger.LogError("An exception occurred while trying to log into Lavalink", ex);
                    return;
                }
            });
        });

        while (!loadDatabase.IsCompleted || !logInToDiscord.IsCompleted)
            await Task.Delay(100);

        if (!loadDatabase.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the database.", loadDatabase.Exception);
            await Task.Delay(1000);
            Environment.Exit(ExitCodes.FailedDatabaseLoad);
        }

        if (!logInToDiscord.IsCompletedSuccessfully)
        {
            _logger.LogFatal("An uncaught exception occurred while initializing the discord client.", logInToDiscord.Exception);
            await Task.Delay(1000);
            Environment.Exit(ExitCodes.FailedDiscordLogin);
        }

        watcher.Watcher();

        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            ExitApplication(true).Wait();
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
                    _logger.LogError("Failed to update user status", ex);
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

    private async Task ExecuteSyncTasks(IReadOnlyDictionary<ulong, DiscordGuild> Guilds)
    {
        ObservableList<Task> runningTasks = new();

        void runningTasksUpdated(object sender, ObservableListUpdate<Task> e)
        {
            if (e is not null && e.NewItems is not null)
                foreach (Task b in e.NewItems)
                {
                    b.Add(watcher);
                }
        }

        runningTasks.ItemsChanged += runningTasksUpdated;

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
                _logger.LogDebug("Performing sync tasks for '{guild}'..", guild.Key);

                if (objectedUsers.Contains(guild.Value.OwnerId) || bannedUsers.ContainsKey(guild.Value.OwnerId) || bannedGuilds.ContainsKey(guild.Key))
                {
                    _logger.LogInfo("Leaving guild '{guild}'..", guild.Key);
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

                if (guilds[guild.Key].InviteTracker.Enabled)
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

                    _logger.LogDebug("Requesting more threads for '{guild}'", guild.Key);
                }

                foreach (var b in Threads.Where(x => x.CurrentMember is null))
                {
                    _logger.LogDebug("Joining thread on '{guild}': {thread}", guild.Key, b.Id);
                    b.JoinWithQueue(threadJoinClient);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to join threads on '{guild}'", ex, guild.Key);
            }
        }

        while (runningTasks.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        runningTasks.ItemsChanged -= runningTasksUpdated;
        runningTasks.Clear();

        _logger.LogInfo("Sync Tasks successfully finished for {startupTasksSuccess}/{GuildCount} guilds.", startupTasksSuccess, Guilds.Count);
        _ = databaseClient.FullSyncDatabase();
    }

    private async Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Run(async () =>
        {
            status.DiscordGuildDownloadCompleted = true;

            _logger.LogInfo("I'm on {GuildsCount} guilds.", e.Guilds.Count);

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
                                    _logger.LogInfo("Fetching video length for '{Url}'", b.Url);

                                    var track = await discordClient.GetLavalink().ConnectedNodes.First(x => x.Value.IsConnected).Value.Rest.GetTracksAsync(b.Url, LavalinkSearchType.Plain);

                                    if (track.LoadResultType != LavalinkLoadResultType.TrackLoaded)
                                    {
                                        list.List.Remove(b);
                                        _logger.LogError("Failed to load video length for '{Url}'", track.Exception, b.Url);
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

            for (int i = 0; i < 501; i++)
            {
                experienceHandler.CalculateLevelRequirement(i);
            }

            foreach (var guild in e.Guilds)
            {
                if (!guilds.ContainsKey(guild.Key))
                    guilds.Add(guild.Key, new Guild(guild.Key, this));

                if (guilds[guild.Key].BumpReminder.Enabled)
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

                if (guilds[guild.Key].Crosspost.CrosspostChannels.Any())
                {
                    Task.Run(async () =>
                    {
                        for (int i = 0; i < guilds[guild.Key].Crosspost.CrosspostChannels.Count; i++)
                        {
                            if (guild.Value is null)
                                return;

                            var ChannelId = guilds[guild.Key].Crosspost.CrosspostChannels[i];

                            _logger.LogDebug("Checking channel '{ChannelId}' for missing crossposts..", ChannelId);

                            if (!guild.Value.Channels.ContainsKey(ChannelId))
                                return;

                            var Messages = await guild.Value.GetChannel(ChannelId).GetMessagesAsync(20);

                            if (Messages.Any(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                foreach (var msg in Messages.Where(x => x.Flags.HasValue && !x.Flags.Value.HasMessageFlag(MessageFlags.Crossposted)))
                                {
                                    _logger.LogDebug("Handling missing crosspost message '{msg}' in '{ChannelId}' for '{guild}'..", msg.Id, msg.ChannelId, guild.Key);

                                    var WaitTime = guilds[guild.Value.Id].Crosspost.DelayBeforePosting - msg.Id.GetSnowflakeTime().GetTotalSecondsSince();

                                    if (WaitTime > 0)
                                        await Task.Delay(TimeSpan.FromSeconds(WaitTime));

                                    if (guilds[guild.Value.Id].Crosspost.DelayBeforePosting > 3)
                                        _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode("🕒"));

                                    bool ReactionAdded = false;

                                    var task = guilds[guild.Value.Id].Crosspost.CrosspostWithRatelimit(msg.Channel, msg).ContinueWith(s =>
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
                await ExecuteSyncTasks(discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

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
                        if (guilds[guild.Key].MusicModule.ChannelId != 0)
                        {
                            if (!guild.Value.Channels.ContainsKey(guilds[guild.Key].MusicModule.ChannelId))
                                throw new Exception("Channel no longer exists");

                            if (guilds[guild.Key].MusicModule.CurrentVideo.ToLower().Contains("localhost") || guilds[guild.Key].MusicModule.CurrentVideo.ToLower().Contains("127.0.0.1"))
                                throw new Exception("Localhost?");

                            var channel = guild.Value.GetChannel(guilds[guild.Key].MusicModule.ChannelId);

                            if (!channel.Users.Where(x => !x.IsBot).Any())
                                throw new Exception("Channel empty");

                            if (guilds[guild.Key].MusicModule.SongQueue.Count > 0)
                            {
                                for (var i = 0; i < guilds[guild.Key].MusicModule.SongQueue.Count; i++)
                                {
                                    Lavalink.QueueInfo b = guilds[guild.Key].MusicModule.SongQueue[i];

                                    _logger.LogDebug("Fixing queue info for '{Url}'", b.Url);

                                    b.guild = guild.Value;

                                    if (!UserCache.Any(x => x.Id == b.UserId))
                                    {
                                        _logger.LogDebug("Fetching user '{UserId}'", b.UserId);
                                        UserCache.Add(await discordClient.GetUserAsync(b.UserId));
                                    }

                                    b.user = UserCache.First(x => x.Id == b.UserId);
                                }
                            }

                            var lava = discordClient.GetLavalink();

                            while (!lava.ConnectedNodes.Values.Any(x => x.IsConnected))
                                await Task.Delay(1000);

                            var node = lava.ConnectedNodes.Values.First(x => x.IsConnected);
                            var conn = node.GetGuildConnection(guild.Value);

                            if (conn is null)
                            {
                                if (!lava.ConnectedNodes.Any())
                                {
                                    throw new Exception("Lavalink connection isn't established.");
                                }

                                conn = await node.ConnectAsync(channel);
                            }

                            var loadResult = await node.Rest.GetTracksAsync(guilds[guild.Key].MusicModule.CurrentVideo, LavalinkSearchType.Plain);

                            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                                return;

                            await conn.PlayAsync(loadResult.Tracks.First());

                            await Task.Delay(2000);
                            await conn.SeekAsync(TimeSpan.FromSeconds(guilds[guild.Key].MusicModule.CurrentVideoPosition));

                            guilds[guild.Key].MusicModule.QueueHandler(this, discordClient, node, conn);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("An exception occurred while trying to continue music playback for '{guild}'", ex, guild.Key);
                        guilds[guild.Key].MusicModule = new(guilds[guild.Key]);
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

        _logger.LogInfo("Preparing to shut down Ichigo..");

        if (status.DiscordInitialized && !Immediate)
        {
            try
            {
                Stopwatch sw = new();
                sw.Start();

                if (!status.DiscordCommandsRegistered)
                    _logger.LogWarn("Startup is incomplete. Waiting for Startup to finish to shutdown..");

                while (!status.DiscordCommandsRegistered && sw.ElapsedMilliseconds < TimeSpan.FromMinutes(5).TotalMilliseconds)
                    await Task.Delay(500);

                await ExecuteSyncTasks(discordClient.Guilds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to run sync tasks", ex);
            }

            try
            {
                _logger.LogInfo("Closing Discord Client..");

                await discordClient.UpdateStatusAsync(userStatus: UserStatus.Offline);
                await discordClient.DisconnectAsync();

                _logger.LogDebug("Closed Discord Client.");
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
                _logger.LogInfo("Flushing to database..");
                await databaseClient.FullSyncDatabase(true);
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

    private async void LogHandler(object? sender, LogMessageEventArgs e)
    {
        switch (e.LogEntry.LogLevel)
        {
            case LogLevel.FATAL:
            case LogLevel.ERROR:
            {
                try
                {
                    if (status.DiscordInitialized)
                    {
                        if (e.LogEntry.Message is "[111] Connection terminated (4000, ''), reconnecting" or "[111] Connection terminated (-1, ''), reconnecting")
                            break;

                        var channel = discordClient.Guilds[status.LoadedConfig.Channels.Assets].GetChannel(status.LoadedConfig.Channels.ExceptionLog);

                        _ = channel.SendMessageAsync(new DiscordEmbedBuilder()
                            .WithColor(e.LogEntry.LogLevel == LogLevel.FATAL ? new DiscordColor("#FF0000") : EmbedColors.Error)
                            .WithTitle(e.LogEntry.LogLevel.GetName().ToLower().FirstLetterToUpper())
                            .WithDescription($"```\n{e.LogEntry.Message.SanitizeForCode()}\n```{(e.LogEntry.Exception is not null ? $"\n```cs\n{e.LogEntry.Exception.ToString().SanitizeForCode()}```" : "")}")
                            .WithTimestamp(e.LogEntry.TimeOfEvent));
                    }
                }
                catch {}
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
                break;
            }
            default:
                break;
        }
    }
}