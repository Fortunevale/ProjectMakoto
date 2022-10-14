using ProjectIchigo.Entities.Database;

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
        IEnumerable<TableDefinitions.scam_urls> scam_urls = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.scam_urls>(_bot.databaseClient._helper.GetLoadCommand("scam_urls"));

        foreach (var b in scam_urls)
            _bot.phishingUrls.Add(b.url, new PhishingUrlEntry
            {
                Url = b.url,
                Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                Submitter = b.submitter
            });
        _logger.LogDebug($"Loaded {_bot.phishingUrls.Count} malicious urls");

        IEnumerable<TableDefinitions.guilds> guilds = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.guilds>(_bot.databaseClient._helper.GetLoadCommand("guilds"));

        foreach (var b in guilds)
        {
            var DbGuild = new Guild(b.serverid, _bot);
            _bot.guilds.Add(b.serverid, DbGuild);

            DbGuild.TokenLeakDetection = new(DbGuild)
            {
                DetectTokens = b.tokens_detect
            };

            DbGuild.PhishingDetection = new(DbGuild)
            {
                DetectPhishing = b.phishing_detect,
                WarnOnRedirect = b.phishing_warnonredirect,
                AbuseIpDbReports = b.phishing_abuseipdb,
                PunishmentType = (PhishingPunishmentType)((int)b.phishing_type),
                CustomPunishmentReason = b.phishing_reason,
                CustomPunishmentLength = TimeSpan.FromSeconds((long)b.phishing_time)
            };

            DbGuild.BumpReminder = new(DbGuild)
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

            DbGuild.Join = new(DbGuild)
            {
                AutoAssignRoleId = b.auto_assign_role_id,
                JoinlogChannelId = b.joinlog_channel_id,
                AutoBanGlobalBans = b.autoban_global_ban,
                ReApplyRoles = b.reapplyroles,
                ReApplyNickname = b.reapplynickname,
            };

            DbGuild.Experience = new(DbGuild)
            {
                UseExperience = b.experience_use,
                BoostXpForBumpReminder = b.experience_boost_bumpreminder
            };

            DbGuild.Crosspost = new(DbGuild)
            {
                CrosspostChannels = JsonConvert.DeserializeObject<List<ulong>>(b.crosspostchannels) ?? new(),
                DelayBeforePosting = b.crosspostdelay,
                ExcludeBots = b.crosspostexcludebots,
                CrosspostRatelimits = JsonConvert.DeserializeObject<Dictionary<ulong, CrosspostRatelimit>>(b.crosspost_ratelimits) ?? new(),
            };

            DbGuild.ActionLog = new(DbGuild)
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

            DbGuild.InviteTracker = new(DbGuild)
            {
                Enabled = b.invitetracker_enabled,
                Cache = JsonConvert.DeserializeObject<List<InviteTrackerCacheItem>>(b.invitetracker_cache) ?? new()
            };

            DbGuild.InviteNotes = new(DbGuild)
            {
                Notes = JsonConvert.DeserializeObject<List<InviteNotesDetails>>(b.invitenotes) ?? new()
            };

            DbGuild.InVoiceTextPrivacy = new(DbGuild)
            {
                ClearTextEnabled = b.vc_privacy_clear,
                SetPermissionsEnabled = b.vc_privacy_perms
            };

            DbGuild.NameNormalizer = new(DbGuild)
            {
                NameNormalizerEnabled = b.normalizenames
            };
            
            DbGuild.EmbedMessage = new(DbGuild)
            {
                UseEmbedding = b.embed_messages,
                UseGithubEmbedding = b.embed_github
            };

            if (b.lavalink_channel != 0)
                DbGuild.MusicModule = new(DbGuild)
                {
                    ChannelId = b.lavalink_channel,
                    CurrentVideoPosition = b.lavalink_currentposition,
                    CurrentVideo = b.lavalink_currentvideo,
                    IsPaused = b.lavalink_paused,
                    Shuffle = b.lavalink_shuffle,
                    Repeat = b.lavalink_repeat,
                    SongQueue = JsonConvert.DeserializeObject<List<Lavalink.QueueInfo>>(b.lavalink_queue) ?? new()
                };
            else
                DbGuild.MusicModule = new(DbGuild);

            DbGuild.Polls = new(DbGuild, _bot);
            foreach (var c in JsonConvert.DeserializeObject<List<PollEntry>>(b.polls) ?? new())
                DbGuild.Polls.RunningPolls.Add(c);

            DbGuild.LevelRewards = JsonConvert.DeserializeObject<List<LevelRewardEntry>>(b.levelrewards) ?? new();
            DbGuild.ActionLog.ProcessedAuditLogs = JsonConvert.DeserializeObject<ObservableList<ulong>>(b.auditlogcache) ?? new();
            DbGuild.ReactionRoles = JsonConvert.DeserializeObject<List<KeyValuePair<ulong, ReactionRoleEntry>>>(b.reactionroles) ?? new();
            DbGuild.AutoUnarchiveThreads = JsonConvert.DeserializeObject<List<ulong>>(b.autounarchivelist) ?? new();
        }
        _logger.LogDebug($"Loaded {_bot.guilds.Count} guilds");


        foreach (var table in await _bot.databaseClient._helper.ListTables(_bot.databaseClient.guildDatabaseConnection))
        {
            if (table.IsDigitsOnly())
            {
                IEnumerable<TableDefinitions.guild_users> memberList = _bot.databaseClient.guildDatabaseConnection.Query<TableDefinitions.guild_users>(_bot.databaseClient._helper.GetLoadCommand(table, "guild_users"));

                if (!_bot.guilds.ContainsKey(Convert.ToUInt64(table)))
                {
                    _logger.LogWarn($"Table '{table}' has no server attached to it. Dropping table.");
                    await _bot.databaseClient._helper.DropTable(_bot.databaseClient.guildDatabaseConnection, table);
                    continue;
                }

                foreach (var b in memberList)
                {
                    Member DbUser = new(_bot.guilds[Convert.ToUInt64(table)], b.userid);
                    _bot.guilds[Convert.ToUInt64(table)].Members.Add(b.userid, DbUser);

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
                    DbUser.MemberRoles = JsonConvert.DeserializeObject<List<MemberRole>>(b.roles) ?? new List<MemberRole>();
                    DbUser.SavedNickname = b.saved_nickname ?? "";
                }

                _logger.LogDebug($"Loaded {_bot.guilds[Convert.ToUInt64(table)].Members.Count} members for {table}");
            }
        }


        IEnumerable<TableDefinitions.users> users = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.users>(_bot.databaseClient._helper.GetLoadCommand("users"));

        foreach (var b in users)
        {
            _bot.users.Add(b.userid, new User(_bot, b.userid));

            var DbUser = _bot.users[b.userid];

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
            DbUser.ExperienceUser = new(DbUser)
            {
                DirectMessageOptOut = b.experience_directmessageoptout
            };
            DbUser.UserPlaylists = JsonConvert.DeserializeObject<List<UserPlaylist>>(b.playlists) ?? new();

            foreach (var c in JsonConvert.DeserializeObject<List<ReminderItem>>(b.reminders) ?? new())
                DbUser.Reminders.ScheduledReminders.Add(c);
        }
        _logger.LogDebug($"Loaded {_bot.users.Count} users");

        IEnumerable<ulong> objected_users = _bot.databaseClient.mainDatabaseConnection.Query<ulong>(_bot.databaseClient._helper.GetLoadCommand("objected_users"));

        _bot.objectedUsers = objected_users.ToList();
        _logger.LogDebug($"Loaded {_bot.objectedUsers.Count} objected users");

        IEnumerable<TableDefinitions.globalbans> globalbans = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.globalbans>(_bot.databaseClient._helper.GetLoadCommand("globalbans"));

        foreach (var b in globalbans)
            _bot.globalBans.Add(b.id, new GlobalBanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator,
                Timestamp = (b.timestamp == 0 ? DateTime.UtcNow : new DateTime().ToUniversalTime().AddTicks((long)b.timestamp)),
            });
        _logger.LogDebug($"Loaded {_bot.globalBans.Count} global bans");

        IEnumerable<TableDefinitions.banned_users> banned_users = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.banned_users>(_bot.databaseClient._helper.GetLoadCommand("banned_users"));

        foreach (var b in banned_users)
            _bot.bannedUsers.Add(b.id, new BlacklistEntry
            {
                Reason = b.reason,
                Moderator = b.moderator,
                Timestamp = (b.timestamp == 0 ? DateTime.UtcNow : new DateTime().ToUniversalTime().AddTicks((long)b.timestamp)),
            });

        _logger.LogDebug($"Loaded {_bot.bannedUsers.Count} user bans");
        
        IEnumerable<TableDefinitions.banned_guilds> banned_guilds = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.banned_guilds>(_bot.databaseClient._helper.GetLoadCommand("banned_guilds"));

        foreach (var b in banned_guilds)
            _bot.bannedGuilds.Add(b.id, new BlacklistEntry
            {
                Reason = b.reason,
                Moderator = b.moderator,
                Timestamp = (b.timestamp == 0 ? DateTime.UtcNow : new DateTime().ToUniversalTime().AddTicks((long)b.timestamp)),
            });
        _logger.LogDebug($"Loaded {_bot.bannedGuilds.Count} guild bans");


        IEnumerable<TableDefinitions.submission_user_bans> submission_user_bans = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.submission_user_bans>(_bot.databaseClient._helper.GetLoadCommand("submission_user_bans"));

        foreach (var b in submission_user_bans)
            _bot.phishingUrlSubmissionUserBans.Add(b.id, new PhishingSubmissionBanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogDebug($"Loaded {_bot.phishingUrlSubmissionUserBans.Count} user submission bans");


        IEnumerable<TableDefinitions.submission_guild_bans> submission_guild_bans = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.submission_guild_bans>(_bot.databaseClient._helper.GetLoadCommand("submission_guild_bans"));

        foreach (var b in submission_guild_bans)
            _bot.phishingUrlSubmissionGuildBans.Add(b.id, new PhishingSubmissionBanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogDebug($"Loaded {_bot.phishingUrlSubmissionGuildBans.Count} guild submission bans");


        IEnumerable<TableDefinitions.active_url_submissions> active_url_submissions = _bot.databaseClient.mainDatabaseConnection.Query<TableDefinitions.active_url_submissions>(_bot.databaseClient._helper.GetLoadCommand("active_url_submissions"));

        foreach (var b in active_url_submissions)
            _bot.submittedUrls.Add(b.messageid, new SubmittedUrlEntry
            {
                Url = b.url,
                Submitter = b.submitter,
                GuildOrigin = b.guild
            });

        _logger.LogDebug($"Loaded {_bot.submittedUrls.Count} active submissions");
    }
}
