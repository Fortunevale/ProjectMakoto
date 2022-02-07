namespace Project_Ichigo;

internal class Bot
{
    internal DiscordClient? discordClient;
    internal LavalinkNodeConnection? LavalinkNodeConnection;

    internal static MySqlConnection? databaseConnection;
    


    internal static Status _status = new();

    internal static Settings _guilds = new();
    internal static Users _users = new();

    internal static PhishingUrls _phishingUrls = new();
    internal static SubmissionBans _submissionBans = new();
    internal static SubmittedUrls _submittedUrls = new();
    

    internal static DatabaseHelper _databaseHelper = new();


    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.DEBUG, DateTime.UtcNow.AddDays(-3), false);

        LogInfo("Starting up..");

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

        var logInToDiscord = Task.Run(async () =>
        {
            string token = "";

            try
            {
                if (args.Contains("--token"))
                    token = args[ Array.IndexOf(args, "--token") + 1 ];
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to parse a token commandline argument: {ex}");
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

            LogDebug($"Registering DiscordClient..");

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = $"{token}",
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Warning,
                Intents = DiscordIntents.All,
                LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                AutoReconnect = true
            });

            LogDebug($"Registering CommandsNext..");

            discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "-" },
                EnableDefaultHelp = false,
                EnableMentionPrefix = false,
                IgnoreExtraArguments = true,
                EnableDms = false
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
            discordClient.GetCommandsNext().RegisterCommands<User>();
            discordClient.GetCommandsNext().RegisterCommands<Mod>();
            discordClient.GetCommandsNext().RegisterCommands<Admin>();

            discordClient.GetCommandsNext().RegisterCommands<Test>();

            LogDebug($"Registering Command Converters..");
            discordClient.GetCommandsNext().RegisterConverter(new CustomArgumentConverter.DiscordUserConverter());
            discordClient.GetCommandsNext().RegisterConverter(new CustomArgumentConverter.BoolConverter());

            LogDebug($"Registering Command Events..");

            CommandEvents commandEvents = new();
            discordClient.GetCommandsNext().CommandExecuted += commandEvents.CommandExecuted;
            discordClient.GetCommandsNext().CommandErrored += commandEvents.CommandError;

            LogDebug($"Registering Phishing Events..");

            PhishingProtectionEvents phishingProtectionEvents = new();
            discordClient.MessageCreated += phishingProtectionEvents.MessageCreated;
            discordClient.MessageUpdated += phishingProtectionEvents.MessageUpdated;

            LogDebug($"Registering Discord Events..");

            DiscordEvents discordEvents = new();
            discordClient.GuildCreated += discordEvents.GuildCreated;

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

                _ = Task.Run(() =>
                {
                    try
                    {
                        _status.TeamMembers.AddRange(discordClient.CurrentApplication.Team.Members.Select(x => x.User.Id));
                        LogInfo($"Added {_status.TeamMembers.Count} users to administrator list");
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured trying to add team members to administrator list. Is the current bot registered in a team?: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                LogError($"An exception occured while trying to log into discord: {ex}");
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
                    LogError($"An exception occured while trying to log into Lavalink: {ex}");
                    return;
                }
            });
        });

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                Stopwatch databaseConnectionSc = new();
                databaseConnectionSc.Start();

                LogInfo($"Connecting to database..");
                databaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};");
                databaseConnection.Open();

                await databaseConnection.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS {Secrets.Secrets.DatabaseName}");
                await databaseConnection.ExecuteAsync($"USE {Secrets.Secrets.DatabaseName}");

                databaseConnectionSc.Stop();
                LogInfo($"Connected to database. ({databaseConnectionSc.ElapsedMilliseconds}ms)");
                _status.DatabaseInitialized = true;
            }
            catch (Exception ex)
            {
                LogFatal($"An exception occured while trying to establish a connection to the database: {ex}");
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLogin);
            }

            try
            {
                List<string> SavedTables = new();

                using (IDataReader reader = databaseConnection.ExecuteReader($"SHOW TABLES"))
                {
                    while (reader.Read())
                    {
                        SavedTables.Add(reader.GetString(0));
                    }
                }

                LogDebug($"Loading phishing urls from table 'scam_urls'..");

                IEnumerable<DatabasePhishingUrlInfo> scamUrls = databaseConnection.Query<DatabasePhishingUrlInfo>($"SELECT url, origin, submitter FROM scam_urls");

                foreach (var b in scamUrls)
                    _phishingUrls.List.Add(b.url, new PhishingUrls.UrlInfo
                    {
                        Url = b.url,
                        Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                        Submitter = b.submitter
                    });

                LogInfo($"Loaded {_phishingUrls.List.Count} phishing urls from table 'scam_urls'.");



                LogDebug($"Loading guilds from table 'guilds'..");

                IEnumerable<DatabaseServerSettings> serverSettings = databaseConnection.Query<DatabaseServerSettings>($"SELECT serverid, phishing_detect, phishing_type, phishing_reason, phishing_time FROM guilds");

                foreach (var b in serverSettings)
                    _guilds.Servers.Add(b.serverid, new Settings.ServerSettings
                    {
                        PhishingDetectionSettings = new()
                        {
                            DetectPhishing = b.phishing_detect,
                            PunishmentType = (Settings.PhishingPunishmentType)b.phishing_type,
                            CustomPunishmentReason = b.phishing_reason,
                            CustomPunishmentLength = TimeSpan.FromSeconds(b.phishing_time)
                        }
                    });

                LogInfo($"Loaded {_guilds.Servers.Count} guilds from table 'guilds'.");



                LogDebug($"Loading users from table 'users'..");

                IEnumerable<DatabaseUsers> users = databaseConnection.Query<DatabaseUsers>($"SELECT userid, submission_accepted_tos, submission_accepted_submissions, submission_last_datetime FROM users");

                foreach (var b in users)
                    _users.List.Add(b.userid, new Users.Info
                    {
                        UrlSubmissions = new()
                        {
                            AcceptedSubmissions = JsonConvert.DeserializeObject<List<string>>(b.submission_accepted_submissions),
                            LastTime = b.submission_last_datetime,
                            AcceptedTOS = b.submission_accepted_tos
                        }
                    });

                LogInfo($"Loaded {_users.List.Count} users from table 'users'.");



                LogDebug($"Loading submission bans from table 'user_submission_bans'..");

                IEnumerable<DatabaseBanInfo> userbans = databaseConnection.Query<DatabaseBanInfo>($"SELECT id, reason, moderator FROM user_submission_bans");

                foreach (var b in userbans)
                    _submissionBans.BannedUsers.Add(b.id, new SubmissionBans.BanInfo
                    {
                        Reason = b.reason,
                        Moderator = b.moderator
                    });

                LogInfo($"Loaded {_submissionBans.BannedUsers.Count} submission bans from table 'user_submission_bans'.");



                LogDebug($"Loading submission bans from table 'guild_submission_bans'..");

                IEnumerable<DatabaseBanInfo> guildbans = databaseConnection.Query<DatabaseBanInfo>($"SELECT id, reason, moderator FROM guild_submission_bans");

                foreach (var b in userbans)
                    _submissionBans.BannedGuilds.Add(b.id, new SubmissionBans.BanInfo
                    {
                        Reason = b.reason,
                        Moderator = b.moderator
                    });

                LogInfo($"Loaded {_submissionBans.BannedUsers.Count} submission bans from table 'guild_submission_bans'.");

                _ = new PhishingUrlUpdater().UpdatePhishingUrlDatabase(_phishingUrls);
            }
            catch (Exception ex)
            {
                LogFatal($"An exception occured while trying get data from the database: {ex}");
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLoad);
            }
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

        _ = _databaseHelper.QueueWatcher();

        await Task.Delay(-1);
    }

    private async Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            LogInfo($"I'm on {e.Guilds.Count} guilds.");

            foreach (var guild in e.Guilds)
            {
                if (!Bot._guilds.Servers.ContainsKey(guild.Key))
                    Bot._guilds.Servers.Add(guild.Key, new Settings.ServerSettings());
            }
        });
    }
}