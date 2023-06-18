// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database;

namespace ProjectMakoto.Database;

internal sealed class DatabaseInit : RequiresBotReference
{
    public DatabaseInit(Bot bot) : base(bot)
    {
    }

    internal async Task LoadValuesFromDatabase()
    {
        IEnumerable<TableDefinitions.scam_urls> scam_urls = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.scam_urls>(this.Bot.DatabaseClient._helper.GetLoadCommand("scam_urls"));

        foreach (var b in scam_urls)
            this.Bot.PhishingHosts.Add(b.url, new PhishingUrlEntry
            {
                Url = b.url,
                Origin = JsonConvert.DeserializeObject<List<string>>(b.origin),
                Submitter = b.submitter
            });
        _logger.LogDebug("Loaded {Count} malicious urls", this.Bot.PhishingHosts.Count);

        IEnumerable<TableDefinitions.guilds> guilds = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.guilds>(this.Bot.DatabaseClient._helper.GetLoadCommand("guilds"));

        foreach (var b in guilds)
        {
            var DbGuild = new Guild(b.serverid, this.Bot);
            this.Bot.Guilds.Add(b.serverid, DbGuild);

            DbGuild.PrefixSettings = new(this.Bot, DbGuild)
            {
                Prefix = b.prefix,
                PrefixDisabled = b.prefix_disabled
            };

            DbGuild.TokenLeakDetection = new(this.Bot, DbGuild)
            {
                DetectTokens = b.tokens_detect
            };

            DbGuild.PhishingDetection = new(this.Bot, DbGuild)
            {
                DetectPhishing = b.phishing_detect,
                WarnOnRedirect = b.phishing_warnonredirect,
                AbuseIpDbReports = b.phishing_abuseipdb,
                PunishmentType = (PhishingPunishmentType)((int)b.phishing_type),
                CustomPunishmentReason = b.phishing_reason,
                CustomPunishmentLength = TimeSpan.FromSeconds((long)b.phishing_time)
            };

            DbGuild.BumpReminder = new(this.Bot, DbGuild)
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

            DbGuild.Join = new(this.Bot, DbGuild)
            {
                AutoAssignRoleId = b.auto_assign_role_id,
                JoinlogChannelId = b.joinlog_channel_id,
                AutoBanGlobalBans = b.autoban_global_ban,
                ReApplyRoles = b.reapplyroles,
                ReApplyNickname = b.reapplynickname,
            };

            DbGuild.Experience = new(this.Bot, DbGuild)
            {
                UseExperience = b.experience_use,
                BoostXpForBumpReminder = b.experience_boost_bumpreminder
            };

            DbGuild.Crosspost = new(this.Bot, DbGuild)
            {
                CrosspostChannels = JsonConvert.DeserializeObject<List<ulong>>(b.crosspostchannels) ?? new(),
                DelayBeforePosting = b.crosspostdelay,
                ExcludeBots = b.crosspostexcludebots,
                CrosspostRatelimits = JsonConvert.DeserializeObject<Dictionary<ulong, CrosspostRatelimit>>(b.crosspost_ratelimits) ?? new(),
            };

            DbGuild.ActionLog = new(this.Bot, DbGuild)
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

            DbGuild.InviteTracker = new(this.Bot, DbGuild)
            {
                Enabled = b.invitetracker_enabled,
                Cache = JsonConvert.DeserializeObject<List<InviteTrackerCacheItem>>(b.invitetracker_cache) ?? new()
            };

            DbGuild.InviteNotes = new(this.Bot, DbGuild)
            {
                Notes = JsonConvert.DeserializeObject<Dictionary<string, InviteNotesDetails>>(b.invitenotes) ?? new()
            };

            DbGuild.InVoiceTextPrivacy = new(this.Bot, DbGuild)
            {
                ClearTextEnabled = b.vc_privacy_clear,
                SetPermissionsEnabled = b.vc_privacy_perms
            };

            DbGuild.NameNormalizer = new(this.Bot, DbGuild)
            {
                NameNormalizerEnabled = b.normalizenames
            };

            DbGuild.EmbedMessage = new(this.Bot, DbGuild)
            {
                UseEmbedding = b.embed_messages,
                UseGithubEmbedding = b.embed_github
            };

            if (b.lavalink_channel != 0)
                DbGuild.MusicModule = new(this.Bot, DbGuild)
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
                DbGuild.MusicModule = new(this.Bot, DbGuild);

            DbGuild.Polls = new(this.Bot, DbGuild);
            foreach (var c in JsonConvert.DeserializeObject<List<PollEntry>>(b.polls) ?? new())
                DbGuild.Polls.RunningPolls.Add(c);

            DbGuild.VcCreator = new(this.Bot, DbGuild)
            {
                Channel = b.vccreator_channelid
            };

            foreach (var c in JsonConvert.DeserializeObject<Dictionary<ulong, VcCreatorDetails>>(b.vccreator_channellist) ?? new())
                DbGuild.VcCreator.CreatedChannels.Add(c);

            DbGuild.LevelRewards = JsonConvert.DeserializeObject<List<LevelRewardEntry>>(b.levelrewards) ?? new();
            DbGuild.ActionLog.ProcessedAuditLogs = JsonConvert.DeserializeObject<ObservableList<ulong>>(b.auditlogcache) ?? new();
            DbGuild.ReactionRoles = JsonConvert.DeserializeObject<List<KeyValuePair<ulong, ReactionRoleEntry>>>(b.reactionroles) ?? new();
            DbGuild.AutoUnarchiveThreads = JsonConvert.DeserializeObject<List<ulong>>(b.autounarchivelist) ?? new();

            DbGuild.CurrentLocale = b.current_locale;
            DbGuild.OverrideLocale = b.override_locale;
        }
        _logger.LogDebug("Loaded {Count} guilds", this.Bot.Guilds.Count);


        foreach (var table in await this.Bot.DatabaseClient._helper.ListTables(this.Bot.DatabaseClient.guildDatabaseConnection))
        {
            if (table.IsDigitsOnly())
            {
                IEnumerable<TableDefinitions.guild_users> memberList = this.Bot.DatabaseClient.guildDatabaseConnection.Query<TableDefinitions.guild_users>(this.Bot.DatabaseClient._helper.GetLoadCommand(table, "guild_users"));

                if (!this.Bot.Guilds.ContainsKey(Convert.ToUInt64(table)))
                {
                    _logger.LogWarn("Table '{table}' has no server attached to it. Dropping table.", table);
                    await this.Bot.DatabaseClient._helper.DropTable(this.Bot.DatabaseClient.guildDatabaseConnection, table);
                    continue;
                }

                foreach (var b in memberList)
                {
                    Member DbUser = new(this.Bot, this.Bot.Guilds[Convert.ToUInt64(table)], b.userid);
                    this.Bot.Guilds[Convert.ToUInt64(table)].Members.Add(b.userid, DbUser);

                    DbUser.Experience = new(this.Bot, DbUser)
                    {
                        Level = b.experience_level,
                        Points = b.experience,
                        Last_Message = (b.experience_last_message == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.experience_last_message)),
                    };
                    DbUser.InviteTracker = new(this.Bot, DbUser)
                    {
                        Code = b.invite_code,
                        UserId = b.invite_user
                    };
                    DbUser.FirstJoinDate = (b.first_join == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.first_join));
                    DbUser.LastLeaveDate = (b.last_leave == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.last_leave));
                    DbUser.MemberRoles = JsonConvert.DeserializeObject<List<MemberRole>>(b.roles) ?? new List<MemberRole>();
                    DbUser.SavedNickname = b.saved_nickname ?? "";
                }

                _logger.LogDebug("Loaded {MemberCount} members for {table}", this.Bot.Guilds[Convert.ToUInt64(table)].Members.Count, table);
            }
        }


        IEnumerable<TableDefinitions.users> users = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.users>(this.Bot.DatabaseClient._helper.GetLoadCommand("users"));

        foreach (var b in users)
        {
            this.Bot.Users.Add(b.userid, new User(this.Bot, b.userid));

            var DbUser = this.Bot.Users[b.userid];

            DbUser.UrlSubmissions = new(this.Bot, DbUser)
            {
                AcceptedSubmissions = JsonConvert.DeserializeObject<List<string>>(b.submission_accepted_submissions),
                LastTime = new DateTime().ToUniversalTime().AddTicks(b.submission_last_datetime),
                AcceptedTOS = b.submission_accepted_tos
            };
            DbUser.AfkStatus = new(this.Bot, DbUser)
            {
                Reason = b.afk_reason,
                TimeStamp = (b.afk_since == 0 ? DateTime.UnixEpoch : new DateTime().ToUniversalTime().AddTicks((long)b.afk_since)),
                Messages = JsonConvert.DeserializeObject<List<MessageDetails>>(b.afk_pings),
                MessagesAmount = b.afk_pingamount
            };
            DbUser.ScoreSaber = new(this.Bot, DbUser)
            {
                Id = b.scoresaber_id
            };
            DbUser.ExperienceUser = new(this.Bot, DbUser)
            {
                DirectMessageOptOut = b.experience_directmessageoptout
            };
            DbUser.Translation = new(this.Bot, DbUser)
            {
                LastGoogleSource = b.last_google_source,
                LastGoogleTarget = b.last_google_target,
                LastLibreTranslateSource = b.last_libretranslate_source,
                LastLibreTranslateTarget = b.last_libretranslate_target
            };
            DbUser.Data = new()
            {
                DeletionRequested = b.deletion_requested,
                DeletionRequestDate = new DateTime().ToUniversalTime().AddTicks(b.data_deletion_date),
                LastDataRequest = new DateTime().ToUniversalTime().AddTicks(b.last_data_request),
            };

            DbUser.UserPlaylists = JsonConvert.DeserializeObject<List<UserPlaylist>>(b.playlists) ?? new();
            DbUser.CurrentLocale = b.current_locale;
            DbUser.OverrideLocale = b.override_locale;

            foreach (var c in JsonConvert.DeserializeObject<List<ReminderItem>>(b.reminders) ?? new())
                DbUser.Reminders.ScheduledReminders.Add(c);
        }
        _logger.LogDebug("Loaded {Count} users", this.Bot.Users.Count);

        IEnumerable<ulong> objected_users = this.Bot.DatabaseClient.mainDatabaseConnection.Query<ulong>(this.Bot.DatabaseClient._helper.GetLoadCommand("objected_users"));

        this.Bot.objectedUsers = objected_users.ToList();
        _logger.LogDebug("Loaded {Count} objected users", this.Bot.objectedUsers.Count);

        IEnumerable<TableDefinitions.globalbans> globalbans = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.globalbans>(this.Bot.DatabaseClient._helper.GetLoadCommand("globalbans"));

        foreach (var b in globalbans)
            this.Bot.globalBans.Add(b.id, new BanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator,
                Timestamp = (b.timestamp == 0 ? DateTime.UtcNow : new DateTime().ToUniversalTime().AddTicks((long)b.timestamp)),
            });
        _logger.LogDebug("Loaded {Count} global bans", this.Bot.globalBans.Count);

        IEnumerable<TableDefinitions.globalnotes> globalnotes = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.globalnotes>(this.Bot.DatabaseClient._helper.GetLoadCommand("globalnotes"));

        foreach (var b in globalnotes)
            this.Bot.globalNotes.Add(b.id, JsonConvert.DeserializeObject<List<BanDetails>>(b.notes) ?? new());
        _logger.LogDebug("Loaded {Count} global notes", this.Bot.globalBans.Count);

        IEnumerable<TableDefinitions.banned_users> banned_users = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.banned_users>(this.Bot.DatabaseClient._helper.GetLoadCommand("banned_users"));

        foreach (var b in banned_users)
            this.Bot.bannedUsers.Add(b.id, new BanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator
            });

        _logger.LogDebug("Loaded {Count} user bans", this.Bot.bannedUsers.Count);

        IEnumerable<TableDefinitions.banned_guilds> banned_guilds = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.banned_guilds>(this.Bot.DatabaseClient._helper.GetLoadCommand("banned_guilds"));

        foreach (var b in banned_guilds)
            this.Bot.bannedGuilds.Add(b.id, new BanDetails
            {
                Reason = b.reason,
                Moderator = b.moderator
            });
        _logger.LogDebug("Loaded {Count} guild bans", this.Bot.bannedGuilds.Count);

        IEnumerable<TableDefinitions.active_url_submissions> active_url_submissions = this.Bot.DatabaseClient.mainDatabaseConnection.Query<TableDefinitions.active_url_submissions>(this.Bot.DatabaseClient._helper.GetLoadCommand("active_url_submissions"));

        foreach (var b in active_url_submissions)
            this.Bot.SubmittedHosts.Add(b.messageid, new SubmittedUrlEntry
            {
                Url = b.url,
                Submitter = b.submitter,
                GuildOrigin = b.guild
            });

        _logger.LogDebug("Loaded {Count} active submissions", this.Bot.SubmittedHosts.Count);
    }
}
