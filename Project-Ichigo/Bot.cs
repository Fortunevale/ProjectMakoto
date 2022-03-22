namespace Project_Ichigo;

internal class Bot
{
    internal DiscordClient discordClient;
    internal LavalinkNodeConnection? LavalinkNodeConnection;

    internal Status _status = new();

    internal ServerInfo _guilds = new();
    internal Users _users = new();

    internal PhishingUrls _phishingUrls = new();

    internal SubmissionBans _submissionBans = new();
    internal SubmittedUrls _submittedUrls = new();

    internal GlobalBans _globalBans = new();


    internal TaskWatcher.TaskWatcher _watcher = new();

    internal BumpReminder.BumpReminder _bumpReminder { get; set; }
    internal PhishingUrlUpdater _phishingUrlUpdater { get; set; }
    internal ScoreSaberClient _scoreSaberClient { get; set; }
    internal CountryCodes _countryCodes { get; set; }


    internal ExperienceHandler _experienceHandler { get; set; }
    internal static DatabaseClient _databaseClient { get; set; }


    ServiceProvider services { get; set; }

    internal async Task Init(string[] args)
    {
        if (!Directory.Exists("logs"))
            Directory.CreateDirectory("logs");

        StartLogger($"logs/{DateTime.UtcNow:dd-MM-yyyy_HH-mm-ss}.log", LogLevel.INFO, DateTime.UtcNow.AddDays(-3), false);

        LogInfo("Starting up..");

        try
        {
            if (args.Contains("--debug"))
            {
                ChangeLogLevel(LogLevel.DEBUG);
                LogInfo("Debug logs enabled");
            }
        }
        catch (Exception ex)
        {
            LogError($"An exception occured while to enable debug logs: {ex}");
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

        _bumpReminder = new(_watcher, _guilds);

        var loadDatabase = Task.Run(async () =>
        {
            try
            {
                Stopwatch databaseConnectionSc = new();
                databaseConnectionSc.Start();

                LogInfo($"Loading Country Codes..");
                _countryCodes = new();
                List<string[]> cc = JsonConvert.DeserializeObject<List<string[]>>((await new HttpClient().GetStringAsync("https://fortunevale.dd-dns.de/Countries.json")));
                foreach (var b in cc)
                {
                    _countryCodes.List.Add(b[2], new CountryCodes.CountryInfo { Name = b[0], ContinentCode = b[1] });
                }
                LogInfo($"Loaded {_countryCodes.List.Count} countries.");


                LogInfo($"Connecting to database..");

                _databaseClient = await DatabaseClient.InitializeDatabase(_watcher, _guilds, _users, _submissionBans, _submittedUrls, _globalBans);

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
                LogDebug($"Loading phishing urls from table 'scam_urls'..");

                IEnumerable<DatabasePhishingUrlInfo> scamUrls = _databaseClient.mainDatabaseConnection.Query<DatabasePhishingUrlInfo>(_databaseClient._helper.GetLoadCommand("scam_urls", DatabaseColumnLists.scam_urls));

                foreach (var b in scamUrls)
                    _phishingUrls.List.Add(b.url, new PhishingUrls.UrlInfo
                    {
                        Url = b.url,
                        Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                        Submitter = b.submitter
                    });

                LogInfo($"Loaded {_phishingUrls.List.Count} phishing urls from table 'scam_urls'.");



                LogDebug($"Loading guilds from table 'guilds'..");

                IEnumerable<DatabaseServerSettings> serverSettings = _databaseClient.mainDatabaseConnection.Query<DatabaseServerSettings>(_databaseClient._helper.GetLoadCommand("guilds", DatabaseColumnLists.guilds));

                foreach (var b in serverSettings)
                    _guilds.Servers.Add(b.serverid, new ServerInfo.ServerSettings
                    {
                        PhishingDetectionSettings = new()
                        {
                            DetectPhishing = b.phishing_detect,
                            PunishmentType = (PhishingPunishmentType)b.phishing_type,
                            CustomPunishmentReason = b.phishing_reason,
                            CustomPunishmentLength = TimeSpan.FromSeconds(b.phishing_time)
                        },
                        BumpReminderSettings = new()
                        {
                            Enabled = b.bump_enabled,
                            MessageId = b.bump_message,
                            ChannelId = b.bump_channel,
                            LastBump = new DateTime().ToUniversalTime().AddTicks((long)b.bump_last_time),
                            LastReminder = new DateTime().ToUniversalTime().AddTicks((long)b.bump_last_reminder),
                            LastUserId = b.bump_last_user,
                            PersistentMessageId = b.bump_persistent_msg,
                            RoleId = b.bump_role
                        },
                        JoinSettings = new()
                        {
                            AutoAssignRoleId = b.auto_assign_role_id,
                            JoinlogChannelId = b.joinlog_channel_id
                        },
                        ExperienceSettings = new()
                        {
                            UseExperience = b.experience_use,
                            BoostXpForBumpReminder = b.experience_boost_bumpreminder
                        }
                    });

                LogInfo($"Loaded {_guilds.Servers.Count} guilds from table 'guilds'.");

                foreach (var table in await _databaseClient._helper.ListTables(_databaseClient.guildDatabaseConnection))
                {
                    if (table.StartsWith("guild-"))
                    {
                        LogWarn($"Table '{table}' uses old format. Dropping table.");
                        await _databaseClient._helper.DropTable(_databaseClient.guildDatabaseConnection, table);
                        continue;
                    }

                    if (Regex.IsMatch(table, @"^\d+$"))
                    {
                        LogDebug($"Loading members from table '{table}'..");
                        IEnumerable<DatabaseMembers> memberList = _databaseClient.guildDatabaseConnection.Query<DatabaseMembers>(_databaseClient._helper.GetLoadCommand(table, DatabaseColumnLists.guild_users));

                        if (!_guilds.Servers.ContainsKey(Convert.ToUInt64(table)))
                        {
                            LogWarn($"Table '{table}' has no server attached to it. Dropping table.");
                            await _databaseClient._helper.DropTable(_databaseClient.guildDatabaseConnection, table);
                            continue;
                        }

                        foreach (var b in memberList)
                            _guilds.Servers[Convert.ToUInt64(table)].Members.Add(b.userid, new Members
                            {
                                Level = b.experience_level,
                                Experience = b.experience,
                                Last_Message = (b.experience_last_message == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.experience_last_message))
                            });

                        LogInfo($"Loaded {_guilds.Servers[Convert.ToUInt64(table)].Members.Count} members from table '{table}'.");
                    }
                }


                LogDebug($"Loading users from table 'users'..");

                IEnumerable<DatabaseUsers> users = _databaseClient.mainDatabaseConnection.Query<DatabaseUsers>(_databaseClient._helper.GetLoadCommand("users", DatabaseColumnLists.users));

                foreach (var b in users)
                    _users.List.Add(b.userid, new Users.Info
                    {
                        UrlSubmissions = new()
                        {
                            AcceptedSubmissions = JsonConvert.DeserializeObject<List<string>>(b.submission_accepted_submissions),
                            LastTime = b.submission_last_datetime,
                            AcceptedTOS = b.submission_accepted_tos
                        },
                        AfkStatus = new()
                        {
                            Reason = b.afk_reason,
                            TimeStamp = (b.afk_since == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.afk_since)),
                            Messages = JsonConvert.DeserializeObject<List<AfkStatusMessageCache>>(b.afk_pings),
                            MessagesAmount = b.afk_pingamount
                        },
                        ScoreSaber = new()
                        {
                            Id = b.scoresaber_id
                        },
                        ExperienceUserSettings = new()
                        {
                            DirectMessageOptOut = b.experience_directmessageoptout
                        }
                    });

                LogInfo($"Loaded {_users.List.Count} users from table 'users'.");



                LogDebug($"Loading global bans from table 'globalbans'..");

                IEnumerable<DatabaseBanInfo> globalbans = _databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_databaseClient._helper.GetLoadCommand("globalbans", DatabaseColumnLists.globalbans));

                foreach (var b in globalbans)
                    _globalBans.Users.Add(b.id, new GlobalBans.BanInfo
                    {
                        Reason = b.reason,
                        Moderator = b.moderator
                    });

                LogInfo($"Loaded {_globalBans.Users.Count} submission bans from table 'globalbans'.");



                LogDebug($"Loading submission bans from table 'user_submission_bans'..");

                IEnumerable<DatabaseBanInfo> userbans = _databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_databaseClient._helper.GetLoadCommand("user_submission_bans", DatabaseColumnLists.user_submission_bans));

                foreach (var b in userbans)
                    _submissionBans.BannedUsers.Add(b.id, new SubmissionBans.BanInfo
                    {
                        Reason = b.reason,
                        Moderator = b.moderator
                    });

                LogInfo($"Loaded {_submissionBans.BannedUsers.Count} submission bans from table 'user_submission_bans'.");



                LogDebug($"Loading submission bans from table 'guild_submission_bans'..");

                IEnumerable<DatabaseBanInfo> guildbans = _databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_databaseClient._helper.GetLoadCommand("guild_submission_bans", DatabaseColumnLists.guild_submission_bans));

                foreach (var b in guildbans)
                    _submissionBans.BannedGuilds.Add(b.id, new SubmissionBans.BanInfo
                    {
                        Reason = b.reason,
                        Moderator = b.moderator
                    });

                LogInfo($"Loaded {_submissionBans.BannedGuilds.Count} submission bans from table 'guild_submission_bans'.");



                LogDebug($"Loading active submissions from table 'active_url_submissions'..");

                IEnumerable<DatabaseSubmittedUrls> active_submissions = _databaseClient.mainDatabaseConnection.Query<DatabaseSubmittedUrls>(_databaseClient._helper.GetLoadCommand("active_url_submissions", DatabaseColumnLists.active_url_submissions));

                foreach (var b in active_submissions)
                    _submittedUrls.Urls.Add(b.messageid, new SubmittedUrls.UrlInfo
                    {
                        Url = b.url,
                        Submitter = b.submitter,
                        GuildOrigin = b.guild
                    });

                LogInfo($"Loaded {_submittedUrls.Urls.Count} active submissions from table 'active_url_submissions'.");

                _phishingUrlUpdater = new(_databaseClient);
                _ = _phishingUrlUpdater.UpdatePhishingUrlDatabase(_phishingUrls);
            }
            catch (Exception ex)
            {
                LogFatal($"An exception occured while trying get data from the database: {ex}");
                await Task.Delay(5000);
                Environment.Exit(ExitCodes.FailedDatabaseLoad);
            }
        });

        await loadDatabase.WaitAsync(TimeSpan.FromSeconds(30));

        var logInToDiscord = Task.Run(async () =>
        {
            try
            {
                string token = "";

                try
                {
                    if (args.Contains("--token"))
                        token = args[Array.IndexOf(args, "--token") + 1];
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
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                    Intents = DiscordIntents.All,
                    LogTimestampFormat = "dd.MM.yyyy HH:mm:ss",
                    AutoReconnect = true
                });

                _experienceHandler = new(discordClient, _watcher, _guilds, _users);

                LogDebug($"Registering CommandsNext..");

                services = new ServiceCollection()
                    .AddSingleton(discordClient)
                    .AddSingleton(_status)
                    .AddSingleton(_databaseClient)
                    .AddSingleton(_guilds)
                    .AddSingleton(_users)
                    .AddSingleton(_phishingUrls)
                    .AddSingleton(_submissionBans)
                    .AddSingleton(_submittedUrls)
                    .AddSingleton(_watcher)
                    .AddSingleton(_countryCodes)
                    .AddSingleton(_scoreSaberClient)
                    .AddSingleton(_bumpReminder)
                    .AddSingleton(_globalBans)
                    .AddSingleton(_experienceHandler)
                    .BuildServiceProvider();

                string Prefix = ">>";

                bool IsDev = false;
                bool DevOnline = false;

                Task<int> GetPrefix(DiscordMessage message)
                {
                    return Task<int>.Run(() =>
                    {
                        if (!IsDev)
                            if (DevOnline)
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
                    ServiceProvider = services,
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
                cNext.RegisterConverter(new CustomArgumentConverter.DiscordUserConverter());
                cNext.RegisterConverter(new CustomArgumentConverter.BoolConverter());



                LogDebug($"Registering Command Events..");

                CommandEvents commandEvents = new(_watcher);
                cNext.CommandExecuted += commandEvents.CommandExecuted;
                cNext.CommandErrored += commandEvents.CommandError;

                LogDebug($"Registering Afk Events..");

                AfkEvents afkEvents = new(_watcher, _users);
                discordClient.MessageCreated += afkEvents.MessageCreated;



                LogDebug($"Registering Phishing Events..");

                PhishingProtectionEvents phishingProtectionEvents = new(_phishingUrls, _guilds, _watcher);
                discordClient.MessageCreated += phishingProtectionEvents.MessageCreated;
                discordClient.MessageUpdated += phishingProtectionEvents.MessageUpdated;

                SubmissionEvents _submissionEvents = new(_databaseClient, _submittedUrls, _phishingUrls, _status, _submissionBans);
                discordClient.ComponentInteractionCreated += _submissionEvents.ComponentInteractionCreated;



                LogDebug($"Registering Discord Events..");

                DiscordEvents discordEvents = new();
                discordClient.GuildCreated += discordEvents.GuildCreated;



                LogDebug($"Registering Join Events..");

                JoinEvents joinEvents = new(_guilds, _globalBans, _watcher);
                discordClient.GuildMemberAdded += joinEvents.GuildMemberAdded;
                discordClient.GuildMemberRemoved += joinEvents.GuildMemberRemoved;



                LogDebug($"Registering BumpReminder Events..");

                BumpReminderEvents bumpReminderEvents = new(_watcher, _guilds, _bumpReminder, _experienceHandler);
                discordClient.MessageCreated += bumpReminderEvents.MessageCreated;
                discordClient.MessageDeleted += bumpReminderEvents.MessageDeleted;
                discordClient.MessageReactionAdded += bumpReminderEvents.ReactionAdded;
                discordClient.MessageReactionRemoved += bumpReminderEvents.ReactionRemoved;

                LogDebug($"Registering Experience Events..");

                ExperienceEvents experienceEvents = new(_watcher, _guilds, _experienceHandler);
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
                                    DevOnline = ((await discordClient.GetUserAsync(929373806437470260)).Presence.ClientStatus.Desktop.Value != UserStatus.Offline);
                                }
                                catch (Exception ex)
                                {
                                    LogError($"An exception occured while trying to request the status of the developer client: {ex}");
                                }
                                await Task.Delay(10000);
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
            }
            catch (Exception ex)
            {
                LogError($"{ex}");
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

        _ = _databaseClient.QueueWatcher();
        _watcher.Watcher();

        AppDomain.CurrentDomain.ProcessExit += async delegate
        {
            await FlushToDatabase(null, null);
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
                    await FlushToDatabase(null, null);
                    Environment.Exit(0);
                    return;
                }

                await Task.Delay(1000);
            }
        });

        await Task.Delay(-1);
    }

    private async Task FlushToDatabase(object? sender, EventArgs e)
    {
        if (_databaseClient.IsDisposed())
            return;

        LogInfo($"Flushing to database..");
        await _databaseClient.SyncDatabase(true);
        LogDebug($"Flushed to database.");

        LogInfo($"Closing Discord Client..");
        await discordClient.DisconnectAsync();
        LogDebug($"Closed Discord Client.");

        LogInfo($"Closing database..");
        await _databaseClient.Dispose();
        LogDebug($"Closed database.");

        Thread.Sleep(1000);
        LogInfo($"Goodbye!");
    }

    private async Task GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Task.Run(async () =>
        {
            LogInfo($"I'm on {e.Guilds.Count} guilds.");

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
                        LogDebug($"Added {member.Value.Id} to {guild.Key}");
                        _guilds.Servers[guild.Key].Members.Add(member.Value.Id, new());
                    }

                    _experienceHandler.CheckExperience(member.Key, guild.Value);
                }
            }

            await _databaseClient.CheckGuildTables();
            await _databaseClient.SyncDatabase(true);

        }).Add(_watcher);
    }
}