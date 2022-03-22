namespace Project_Ichigo.Initialization;

internal class DatabaseInit
{
    internal Bot _bot { get; set; }

    internal DatabaseInit(Bot _bot)
    {
        this._bot = _bot;
    }

    internal async Task LoadValuesFromDatabase()
    {
        LogDebug($"Loading phishing urls from table 'scam_urls'..");

        IEnumerable<DatabasePhishingUrlInfo> scamUrls = _bot._databaseClient.mainDatabaseConnection.Query<DatabasePhishingUrlInfo>(_bot._databaseClient._helper.GetLoadCommand("scam_urls", DatabaseColumnLists.scam_urls));

        foreach (var b in scamUrls)
            _bot._phishingUrls.List.Add(b.url, new PhishingUrls.UrlInfo
            {
                Url = b.url,
                Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                Submitter = b.submitter
            });

        LogInfo($"Loaded {_bot._phishingUrls.List.Count} phishing urls from table 'scam_urls'.");



        LogDebug($"Loading guilds from table 'guilds'..");

        IEnumerable<DatabaseServerSettings> serverSettings = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseServerSettings>(_bot._databaseClient._helper.GetLoadCommand("guilds", DatabaseColumnLists.guilds));

        foreach (var b in serverSettings)
            _bot._guilds.Servers.Add(b.serverid, new ServerInfo.ServerSettings
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

        LogInfo($"Loaded {_bot._guilds.Servers.Count} guilds from table 'guilds'.");

        foreach (var table in await _bot._databaseClient._helper.ListTables(_bot._databaseClient.guildDatabaseConnection))
        {
            if (table.StartsWith("guild-"))
            {
                LogWarn($"Table '{table}' uses old format. Dropping table.");
                await _bot._databaseClient._helper.DropTable(_bot._databaseClient.guildDatabaseConnection, table);
                continue;
            }

            if (Regex.IsMatch(table, @"^\d+$"))
            {
                LogDebug($"Loading members from table '{table}'..");
                IEnumerable<DatabaseMembers> memberList = _bot._databaseClient.guildDatabaseConnection.Query<DatabaseMembers>(_bot._databaseClient._helper.GetLoadCommand(table, DatabaseColumnLists.guild_users));

                if (!_bot._guilds.Servers.ContainsKey(Convert.ToUInt64(table)))
                {
                    LogWarn($"Table '{table}' has no server attached to it. Dropping table.");
                    await _bot._databaseClient._helper.DropTable(_bot._databaseClient.guildDatabaseConnection, table);
                    continue;
                }

                foreach (var b in memberList)
                    _bot._guilds.Servers[Convert.ToUInt64(table)].Members.Add(b.userid, new Members
                    {
                        Level = b.experience_level,
                        Experience = b.experience,
                        Last_Message = (b.experience_last_message == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.experience_last_message))
                    });

                LogInfo($"Loaded {_bot._guilds.Servers[Convert.ToUInt64(table)].Members.Count} members from table '{table}'.");
            }
        }


        LogDebug($"Loading users from table 'users'..");

        IEnumerable<DatabaseUsers> users = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseUsers>(_bot._databaseClient._helper.GetLoadCommand("users", DatabaseColumnLists.users));

        foreach (var b in users)
            _bot._users.List.Add(b.userid, new Users.Info
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

        LogInfo($"Loaded {_bot._users.List.Count} users from table 'users'.");



        LogDebug($"Loading global bans from table 'globalbans'..");

        IEnumerable<DatabaseBanInfo> globalbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("globalbans", DatabaseColumnLists.globalbans));

        foreach (var b in globalbans)
            _bot._globalBans.Users.Add(b.id, new GlobalBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        LogInfo($"Loaded {_bot._globalBans.Users.Count} submission bans from table 'globalbans'.");



        LogDebug($"Loading submission bans from table 'user_submission_bans'..");

        IEnumerable<DatabaseBanInfo> userbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("user_submission_bans", DatabaseColumnLists.user_submission_bans));

        foreach (var b in userbans)
            _bot._submissionBans.BannedUsers.Add(b.id, new SubmissionBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        LogInfo($"Loaded {_bot._submissionBans.BannedUsers.Count} submission bans from table 'user_submission_bans'.");



        LogDebug($"Loading submission bans from table 'guild_submission_bans'..");

        IEnumerable<DatabaseBanInfo> guildbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("guild_submission_bans", DatabaseColumnLists.guild_submission_bans));

        foreach (var b in guildbans)
            _bot._submissionBans.BannedGuilds.Add(b.id, new SubmissionBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        LogInfo($"Loaded {_bot._submissionBans.BannedGuilds.Count} submission bans from table 'guild_submission_bans'.");



        LogDebug($"Loading active submissions from table 'active_url_submissions'..");

        IEnumerable<DatabaseSubmittedUrls> active_submissions = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseSubmittedUrls>(_bot._databaseClient._helper.GetLoadCommand("active_url_submissions", DatabaseColumnLists.active_url_submissions));

        foreach (var b in active_submissions)
            _bot._submittedUrls.Urls.Add(b.messageid, new SubmittedUrls.UrlInfo
            {
                Url = b.url,
                Submitter = b.submitter,
                GuildOrigin = b.guild
            });

        LogInfo($"Loaded {_bot._submittedUrls.Urls.Count} active submissions from table 'active_url_submissions'.");
    }

    internal async Task UpdateCountryCodes()
    {
        try
        {
            LogInfo($"Loading Country Codes..");
            _bot._countryCodes = new();
            List<string[]> cc = JsonConvert.DeserializeObject<List<string[]>>((await new HttpClient().GetStringAsync("https://fortunevale.dd-dns.de/Countries.json")));
            foreach (var b in cc)
            {
                _bot._countryCodes.List.Add(b[2], new CountryCodes.CountryInfo { Name = b[0], ContinentCode = b[1] });
            }
            LogInfo($"Loaded {_bot._countryCodes.List.Count} countries.");
        }
        catch (Exception ex)
        {
            LogFatal($"An exception occured while trying to load country codes from server: {ex}");
            await Task.Delay(5000);
            throw;
        }
    }
}
