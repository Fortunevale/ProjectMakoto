namespace ProjectIchigo.Database;

internal class DatabaseInit
{
    internal Bot _bot { get; set; }

    internal DatabaseInit(Bot _bot)
    {
        this._bot = _bot;
    }

    internal async Task LoadValuesFromDatabase()
    {
        _logger.LogDebug($"Loading phishing urls from table 'scam_urls'..");

        IEnumerable<DatabasePhishingUrlInfo> scamUrls = _bot._databaseClient.mainDatabaseConnection.Query<DatabasePhishingUrlInfo>(_bot._databaseClient._helper.GetLoadCommand("scam_urls", DatabaseColumnLists.scam_urls));

        foreach (DatabasePhishingUrlInfo b in scamUrls)
            _bot._phishingUrls.List.Add(b.url, new PhishingUrls.UrlInfo
            {
                Url = b.url,
                Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                Submitter = b.submitter
            });

        _logger.LogInfo($"Loaded {_bot._phishingUrls.List.Count} phishing urls from table 'scam_urls'.");



        _logger.LogDebug($"Loading guilds from table 'guilds'..");

        IEnumerable<DatabaseGuildSettings> serverSettings = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseGuildSettings>(_bot._databaseClient._helper.GetLoadCommand("guilds", DatabaseColumnLists.guilds));

        foreach (var b in serverSettings)
        {
            var DbGuild = new Guild(b.serverid);
            _bot._guilds.Add(b.serverid, DbGuild);

            DbGuild.PhishingDetectionSettings = new(DbGuild)
            {
                DetectPhishing = b.phishing_detect,
                WarnOnRedirect = b.phishing_warnonredirect,
                PunishmentType = (PhishingPunishmentType)b.phishing_type,
                CustomPunishmentReason = b.phishing_reason,
                CustomPunishmentLength = TimeSpan.FromSeconds(b.phishing_time)
            };

            DbGuild.BumpReminderSettings = new(DbGuild)
            {
                Enabled = b.bump_enabled,
                MessageId = b.bump_message,
                ChannelId = b.bump_channel,
                LastBump = new DateTime().ToUniversalTime().AddTicks(b.bump_last_time),
                LastReminder = new DateTime().ToUniversalTime().AddTicks(b.bump_last_reminder),
                LastUserId = b.bump_last_user,
                PersistentMessageId = b.bump_persistent_msg,
                RoleId = b.bump_role,
                BumpsMissed = b.bump_missed
            };

            DbGuild.JoinSettings = new(DbGuild)
            {
                AutoAssignRoleId = b.auto_assign_role_id,
                JoinlogChannelId = b.joinlog_channel_id,
                AutoBanGlobalBans = b.autoban_global_ban,
                ReApplyRoles = b.reapplyroles,
                ReApplyNickname = b.reapplynickname,
            };

            DbGuild.ExperienceSettings = new(DbGuild)
            {
                UseExperience = b.experience_use,
                BoostXpForBumpReminder = b.experience_boost_bumpreminder
            };

            DbGuild.CrosspostSettings = new(DbGuild)
            {
                CrosspostChannels = JsonConvert.DeserializeObject<ObservableCollection<ulong>>((b.crosspostchannels is null or "null" or "" ? "[]" : b.crosspostchannels)),
                DelayBeforePosting = b.crosspostdelay,
                ExcludeBots = b.crosspostexcludebots,
                CrosspostTasks = JsonConvert.DeserializeObject<ObservableCollection<CrosspostMessage>>((b.crossposttasks is null or "null" or "" ? "[]" : b.crossposttasks)),
            };

            DbGuild.ActionLogSettings = new(DbGuild)
            {
                Channel = b.actionlog_channel,
                AttemptGettingMoreDetails = b.actionlog_attempt_further_detail,
                MemberModified = b.actionlog_log_member_modified,
                MembersModified = b.actionlog_log_members_modified,
                BanlistModified = b.actionlog_log_banlist_modified,
                GuildModified = b.actionlog_log_guild_modified,
                InvitesModified = b.actionlog_log_invites_modified,
                MessageDeleted = b.actionlog_log_message_deleted,
                MessageModified = b.actionlog_log_message_updated,
                RolesModified = b.actionlog_log_roles_modified,
                MemberProfileModified = b.actionlog_log_memberprofile_modified,
                ChannelsModified = b.actionlog_log_channels_modified,
                VoiceStateUpdated = b.actionlog_log_voice_state,
            };

            DbGuild.InviteTrackerSettings = new(DbGuild)
            {
                Enabled = b.invitetracker_enabled,
                Cache = JsonConvert.DeserializeObject<ObservableCollection<InviteTrackerCacheItem>>((b.invitetracker_cache is null or "null" or "" ? "[]" : b.invitetracker_cache))
            };

            DbGuild.InVoiceTextPrivacySettings = new(DbGuild)
            {
                ClearTextEnabled = b.vc_privacy_clear,
                SetPermissionsEnabled = b.vc_privacy_perms
            };

            DbGuild.NameNormalizerSettings = new(DbGuild)
            {
                NameNormalizerEnabled = b.normalizenames
            };
            
            DbGuild.EmbedMessageSettings = new(DbGuild)
            {
                UseEmbedding = b.embed_messages
            };

            DbGuild.LevelRewards = JsonConvert.DeserializeObject<List<LevelReward>>((b.levelrewards is null or "null" or "" ? "[]" : b.levelrewards));
            DbGuild.ProcessedAuditLogs = JsonConvert.DeserializeObject<ObservableCollection<ulong>>((b.auditlogcache is null or "null" or "" ? "[]" : b.auditlogcache));
            DbGuild.ReactionRoles = JsonConvert.DeserializeObject<List<KeyValuePair<ulong, ReactionRoles>>>((b.reactionroles is null or "null" or "" ? "[]" : b.reactionroles));
            DbGuild.AutoUnarchiveThreads = JsonConvert.DeserializeObject<ObservableCollection<ulong>>((b.autounarchivelist is null or "null" or "" ? "[]" : b.autounarchivelist));
        }

        _logger.LogInfo($"Loaded {_bot._guilds.Count} guilds from table 'guilds'.");

        foreach (var table in await _bot._databaseClient._helper.ListTables(_bot._databaseClient.guildDatabaseConnection))
        {
            if (table.StartsWith("guild-"))
            {
                _logger.LogWarn($"Table '{table}' uses old format. Dropping table.");
                await _bot._databaseClient._helper.DropTable(_bot._databaseClient.guildDatabaseConnection, table);
                continue;
            }

            if (Regex.IsMatch(table, @"^\d+$"))
            {
                _logger.LogDebug($"Loading members from table '{table}'..");
                IEnumerable<DatabaseMembers> memberList = _bot._databaseClient.guildDatabaseConnection.Query<DatabaseMembers>(_bot._databaseClient._helper.GetLoadCommand(table, DatabaseColumnLists.guild_users));

                if (!_bot._guilds.ContainsKey(Convert.ToUInt64(table)))
                {
                    _logger.LogWarn($"Table '{table}' has no server attached to it. Dropping table.");
                    await _bot._databaseClient._helper.DropTable(_bot._databaseClient.guildDatabaseConnection, table);
                    continue;
                }

                foreach (var b in memberList)
                {
                    Member DbUser = new(_bot._guilds[Convert.ToUInt64(table)], b.userid);
                    _bot._guilds[Convert.ToUInt64(table)].Members.Add(b.userid, DbUser);

                    DbUser.Experience = new(DbUser)
                    {
                        Level = b.experience_level,
                        Points = b.experience,
                        Last_Message = (b.experience_last_message == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.experience_last_message)),
                    };
                    DbUser.InviteTracker = new(DbUser)
                    {
                        Code = b.invite_code,
                        UserId = b.invite_user
                    };
                    DbUser.FirstJoinDate = (b.first_join == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.first_join));
                    DbUser.LastLeaveDate = (b.last_leave == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.last_leave));
                    DbUser.MemberRoles = JsonConvert.DeserializeObject<List<MemberRole>>((b.roles is null or "null" or "" ? "[]" : b.roles));
                    DbUser.SavedNickname = b.saved_nickname;
                }

                _logger.LogInfo($"Loaded {_bot._guilds[Convert.ToUInt64(table)].Members.Count} members from table '{table}'.");
            }
        }


        _logger.LogDebug($"Loading users from table 'users'..");

        IEnumerable<DatabaseUsers> users = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseUsers>(_bot._databaseClient._helper.GetLoadCommand("users", DatabaseColumnLists.users));

        foreach (var b in users)
        {
            _bot._users.Add(b.userid, new User(_bot, b.userid));

            var DbUser = _bot._users[b.userid];

            DbUser.UrlSubmissions = new(DbUser)
            {
                AcceptedSubmissions = JsonConvert.DeserializeObject<List<string>>(b.submission_accepted_submissions),
                LastTime = new DateTime().ToUniversalTime().AddTicks(b.submission_last_datetime),
                AcceptedTOS = b.submission_accepted_tos
            };
            DbUser.AfkStatus = new(DbUser)
            {
                Reason = b.afk_reason,
                TimeStamp = (b.afk_since == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.afk_since)),
                Messages = JsonConvert.DeserializeObject<List<MessageDetails>>(b.afk_pings),
                MessagesAmount = b.afk_pingamount
            };
            DbUser.ScoreSaber = new(DbUser)
            {
                Id = b.scoresaber_id
            };
            DbUser.ExperienceUserSettings = new(DbUser)
            {
                DirectMessageOptOut = b.experience_directmessageoptout
            };
            DbUser.UserPlaylists = JsonConvert.DeserializeObject<List<UserPlaylist>>((b.playlists is null or "null" or "" ? "[]" : b.playlists));
        }

        _logger.LogInfo($"Loaded {_bot._users.Count} users from table 'users'.");



        _logger.LogDebug($"Loading global bans from table 'globalbans'..");

        IEnumerable<DatabaseBanInfo> globalbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("globalbans", DatabaseColumnLists.globalbans));

        foreach (var b in globalbans)
            _bot._globalBans.List.Add(b.id, new GlobalBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogInfo($"Loaded {_bot._globalBans.List.Count} submission bans from table 'globalbans'.");



        _logger.LogDebug($"Loading submission bans from table 'user_submission_bans'..");

        IEnumerable<DatabaseBanInfo> userbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("user_submission_bans", DatabaseColumnLists.user_submission_bans));

        foreach (var b in userbans)
            _bot._submissionBans.Users.Add(b.id, new PhishingSubmissionBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogInfo($"Loaded {_bot._submissionBans.Users.Count} submission bans from table 'user_submission_bans'.");



        _logger.LogDebug($"Loading submission bans from table 'guild_submission_bans'..");

        IEnumerable<DatabaseBanInfo> guildbans = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseBanInfo>(_bot._databaseClient._helper.GetLoadCommand("guild_submission_bans", DatabaseColumnLists.guild_submission_bans));

        foreach (var b in guildbans)
            _bot._submissionBans.Guilds.Add(b.id, new PhishingSubmissionBans.BanInfo
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogInfo($"Loaded {_bot._submissionBans.Guilds.Count} submission bans from table 'guild_submission_bans'.");



        _logger.LogDebug($"Loading active submissions from table 'active_url_submissions'..");

        IEnumerable<DatabaseSubmittedUrls> active_submissions = _bot._databaseClient.mainDatabaseConnection.Query<DatabaseSubmittedUrls>(_bot._databaseClient._helper.GetLoadCommand("active_url_submissions", DatabaseColumnLists.active_url_submissions));

        foreach (var b in active_submissions)
            _bot._submittedUrls.List.Add(b.messageid, new SubmittedUrls.UrlInfo
            {
                Url = b.url,
                Submitter = b.submitter,
                GuildOrigin = b.guild
            });

        _logger.LogInfo($"Loaded {_bot._submittedUrls.List.Count} active submissions from table 'active_url_submissions'.");
    }

    internal async Task UpdateCountryCodes()
    {
        try
        {
            _logger.LogInfo($"Loading Country Codes..");
            _bot._countryCodes = new();
            List<string[]> cc = JsonConvert.DeserializeObject<List<string[]>>((await new HttpClient().GetStringAsync("https://fortunevale.dd-dns.de/Countries.json")));
            foreach (var b in cc)
            {
                _bot._countryCodes.List.Add(b[2], new CountryCodes.CountryInfo
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

            _logger.LogInfo($"Loaded {_bot._countryCodes.List.Count} countries.");

            _logger.LogInfo($"Loading Language Codes..");
            _bot._languageCodes = new();
            List<string[]> lc = JsonConvert.DeserializeObject<List<string[]>>((await new HttpClient().GetStringAsync("https://fortunevale.dd-dns.de/Languages.json")));
            foreach (var b in lc)
            {
                _bot._languageCodes.List.Add(new LanguageCodes.LanguageInfo
                {
                    Code = b[0],
                    Name = b[1],
                });
            }
            _logger.LogInfo($"Loaded {_bot._languageCodes.List.Count} languages.");
        }
        catch (Exception ex)
        {
            _logger.LogFatal($"An exception occured while trying to load country codes from server", ex);
            await Task.Delay(5000);
            throw;
        }
    }
}
