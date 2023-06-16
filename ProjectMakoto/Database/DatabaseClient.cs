// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database;
using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Database.ColumnTypes;

namespace ProjectMakoto.Database;

public sealed class DatabaseClient
{
    internal DatabaseClient() { }

    internal MySqlConnection mainDatabaseConnection { get; set; }
    internal MySqlConnection guildDatabaseConnection { get; set; }
    internal DatabaseHelper _helper { get; private set; }
    internal DatabaseQueue _queue { get; private set; }

    private Bot _bot { get; set; }

    private bool Disposed { get; set; } = false;

    internal static async Task<DatabaseClient> InitializeDatabase(Bot _bot)
    {
        _logger.LogInfo("Connecting to database..");

        var databaseClient = new DatabaseClient
        {
            _bot = _bot,

            mainDatabaseConnection = new MySqlConnection($"Server={_bot.status.LoadedConfig.Secrets.Database.Host};Port={_bot.status.LoadedConfig.Secrets.Database.Port};User Id={_bot.status.LoadedConfig.Secrets.Database.Username};Password={_bot.status.LoadedConfig.Secrets.Database.Password};Connection Timeout=60;Database={_bot.status.LoadedConfig.Secrets.Database.MainDatabaseName};"),
            guildDatabaseConnection = new MySqlConnection($"Server={_bot.status.LoadedConfig.Secrets.Database.Host};Port={_bot.status.LoadedConfig.Secrets.Database.Port};User Id={_bot.status.LoadedConfig.Secrets.Database.Username};Password={_bot.status.LoadedConfig.Secrets.Database.Password};Connection Timeout=60;Database={_bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName};")
        };
        databaseClient._helper = new(databaseClient);
        databaseClient._queue = new(_bot);

        databaseClient.mainDatabaseConnection.Open();
        databaseClient.guildDatabaseConnection.Open();

        try
        {
            var MainTables = await databaseClient._helper.ListTables(databaseClient.mainDatabaseConnection);

            foreach (var b in TableDefinitions.TableList)
            {
                if (!MainTables.Contains(b.Name))
                {
                    _logger.LogWarn("Missing table '{Name}'. Creating..", b.Name);
                    string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.MainDatabaseName}`.`{b.Name}` ( {string.Join(", ", b.GetProperties().Select(x => $"`{x.Name}` {x.PropertyType.Name.ToUpper()}{(x.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : "")}{(x.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(x.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out _) ? " NULL" : " NOT NULL")}"))}{(b.GetProperties().Any(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)) ? $", PRIMARY KEY (`{b.GetProperties().First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")})";

                    var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = databaseClient.mainDatabaseConnection;

                    await databaseClient._queue.RunCommand(cmd);
                    _logger.LogInfo("Created table '{Name}'.", b.Name);
                }

                var Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);

                foreach (var col in b.GetProperties())
                {
                    if (!Columns.ContainsKey(col.Name.ToLower()))
                    {
                        _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", col.Name, b.Name);
                        string sql = $"ALTER TABLE `{b.Name}` ADD `{col.Name}` {col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue1) ? $"({maxvalue1.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo("Created column '{Column}' in '{Table}'.", col.Name, b.Name);
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);
                    }

                    if (Columns[col.Name].ToLower() != col.PropertyType.Name.ToLower() + (col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
                    {
                        _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'", col.Name, b.Name);
                        string sql = $"ALTER TABLE `{b.Name}` CHANGE `{col.Name}` `{col.Name}` {col.PropertyType.Name.ToUpper()}{(maxvalue is not null ? $"({maxvalue.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.", col.Name, b.Name, col.PropertyType.Name.ToUpper());
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!b.GetProperties().Any(x => x.Name == col.Key))
                    {
                        _logger.LogWarn("Invalid column '{Column}' in '{Table}'", col.Key, b.Name);

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{b.Name}` DROP COLUMN `{col.Key}`";
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        await databaseClient.CheckGuildTables();

        new Task(new Action(async () =>
        {
            _ = databaseClient.CheckDatabaseConnection(databaseClient.mainDatabaseConnection);
            await Task.Delay(10000);
            _ = databaseClient.CheckDatabaseConnection(databaseClient.guildDatabaseConnection);
        })).CreateScheduledTask(DateTime.UtcNow.AddSeconds(10), "database-connection-watcher");

        Bot.DatabaseClient = databaseClient;
        _bot.status.DatabaseInitialized = true;
        _logger.LogInfo("Connected to database.");
        return databaseClient;
    }

    internal async Task CheckGuildTables()
    {
        while (this._queue.QueueCount != 0)
            await Task.Delay(500);

        IEnumerable<string> GuildTables;

        int retries = 1;

        while (true)
        {
            try
            {
                GuildTables = await this._helper.ListTables(this.guildDatabaseConnection);
                break;
            }
            catch (Exception ex)
            {
                if (retries >= 3)
                {
                    throw;
                }

                _logger.LogWarn("Failed to get a list of guild tables. Retrying in 1000ms.. ({current}/{max})", ex, retries, 3);
                retries++;
                await Task.Delay(1000);
            }
        }

        foreach (var b in this._bot.guilds)
        {
            if (!GuildTables.Contains($"{b.Key}"))
            {
                _logger.LogWarn("Missing table '{Guild}'. Creating..", b.Key);
                string sql = $"CREATE TABLE `{this._bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", typeof(TableDefinitions.guild_users).GetProperties().Select(x => $"`{x.Name}` {x.PropertyType.Name.ToUpper()}{(x.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : "")}{(x.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(x.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out _) ? " NULL" : " NOT NULL")}"))}{(typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)) ? $", PRIMARY KEY (`{typeof(TableDefinitions.guild_users).GetProperties().First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")})";

                var cmd = this.guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = this.guildDatabaseConnection;

                await this._queue.RunCommand(cmd);
                _logger.LogInfo("Created table '{Guild}'.", b.Key);
            }
        }

        GuildTables = await this._helper.ListTables(this.guildDatabaseConnection);

        foreach (var b in GuildTables)
        {
            if (b != "writetester")
            {
                var Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);

                foreach (var col in typeof(TableDefinitions.guild_users).GetProperties())
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        _logger.LogWarn("Missing column '{Column}' in '{Table}'. Creating..", col.Name, b);
                        string sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue1) ? $"({maxvalue1.MaxValue})" : "")}{col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = this.guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = this.guildDatabaseConnection;

                        await this._queue.RunCommand(cmd);

                        _logger.LogInfo("Created column '{Column}' in '{Table}'.", col.Name, b);
                        Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);
                    }

                    if (Columns[col.Name].ToLower() != col.PropertyType.Name.ToLower() + (col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
                    {
                        _logger.LogWarn("Wrong data type for column '{Column}' in '{Table}'", col.Name, b);
                        string sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.PropertyType.Name.ToUpper()}{(maxvalue is not null ? $"({maxvalue.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = this.guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = this.guildDatabaseConnection;

                        await this._queue.RunCommand(cmd);

                        _logger.LogInfo("Changed column '{Column}' in '{Table}' to datatype '{NewDataType}'.", col.Name, b, col.PropertyType.Name.ToUpper());
                        Columns = await this._helper.ListColumns(this.guildDatabaseConnection, b);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.Name == col.Key))
                    {
                        _logger.LogWarn("Invalid column '{Column}' in '{Table}'", col.Key, b);

                        var cmd = this.guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{b}` DROP COLUMN `{col.Key}`";
                        cmd.Connection = this.guildDatabaseConnection;

                        await this._queue.RunCommand(cmd);
                    }
                }
            }
        }
    }

    private async Task CheckDatabaseConnection(MySqlConnection connection)
    {
        new Task(new Action(async () =>
        {
            _ = CheckDatabaseConnection(connection);
        })).CreateScheduledTask(DateTime.UtcNow.AddSeconds(120), "database-connection-watcher");

        if (this.Disposed)
            return;

        while (this._queue.QueueCount > 0)
            await Task.Delay(100);

        if (!await this._queue.RunPing(connection))
        {
            try
            {
                _logger.LogWarn("Pinging the database failed, attempting reconnect.");
                connection.Open();
                _logger.LogInfo("Reconnected to database.");
            }
            catch (Exception ex)
            {
                _logger.LogFatal("Reconnecting to the database failed. Cannot sync changes to database", ex);
                return;
            }
        }

        try
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = this._helper.GetSaveCommand("writetester");

            cmd.CommandText += this._helper.GetValueCommand("writetester", 1);

            cmd.Parameters.AddWithValue($"aaa1", 1);

            cmd.CommandText = cmd.CommandText[..(cmd.CommandText.Length - 2)];
            cmd.CommandText += this._helper.GetOverwriteCommand("writetester");

            cmd.Connection = connection;
            await this._queue.RunCommand(cmd);

            await this._helper.DeleteRow(connection, "writetester", "aaa", "1");
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogWarn("Creating a test value in database failed, reconnecting to database", ex);
                connection.Close();
                connection.Open();
                _logger.LogInfo("Reconnected to database.");
            }
            catch (Exception ex1)
            {
                _logger.LogFatal("Reconnecting to the database failed. Cannot sync changes to database", ex1);
                return;
            }
        }
    }

    private bool RunningFullSync = false;
    private CancellationTokenSource FullSyncCancel = new();
    private DateTimeOffset LastFullSync = DateTimeOffset.MinValue;

    public async Task FullSyncDatabase(bool Important = false)
    {
        if (this.Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (Important && this.RunningFullSync)
        {
            this.FullSyncCancel.Cancel();
            while (this.RunningFullSync)
                await Task.Delay(100);
        }

        if (!Important && this.LastFullSync.GetTimespanSince() < TimeSpan.FromMinutes(20))
            return;

        this.LastFullSync = DateTimeOffset.UtcNow;

        if (this.RunningFullSync)
            return;

        bool IsCancellationRequested()
        {
            if (this.FullSyncCancel.IsCancellationRequested)
            {
                this.RunningFullSync = false;
                this.FullSyncCancel = new();
                return true;
            }

            return false;
        }

        this.RunningFullSync = true;

        _logger.LogDebug("Running full database sync..");

        if (this.mainDatabaseConnection == null || this.guildDatabaseConnection == null)
        {
            throw new Exception($"Exception occurred while trying to update guilds in database: Database mainDatabaseConnection not present");
        }

        List<Task> syncs_running = new();

        async Task SyncTable(MySqlConnection conn, string table, IReadOnlyList<object> DatabaseInserts, string? propertyname = null)
        {
            if (IsCancellationRequested())
                return;

            propertyname ??= table;

            if (!DatabaseInserts.Any())
                return;

            foreach (var chunk in DatabaseInserts.Chunk(3000))
            {
                _logger.LogDebug("Writing to table {table}/{propertyname} with {chunk} inserts", table, propertyname, chunk.Length);

                var cmd = conn.CreateCommand();
                cmd.CommandText = this._helper.GetSaveCommand(table, propertyname);

                for (int i = 0; i < chunk.Length; i++)
                {
                    var b = chunk[i];
                    var properties = b.GetType().GetProperties();

                    cmd.CommandText += this._helper.GetValueCommand(propertyname, i);
                    for (int i1 = 0; i1 < properties.Length; i1++)
                    {
                        var prop = properties[i1];

                        cmd.Parameters.AddWithValue($"{prop.Name}{i}", ((BaseColumn)prop.GetValue(b)).GetValue());
                    }
                    properties = null;
                }

                cmd.CommandText = cmd.CommandText[..(cmd.CommandText.Length - 2)];
                cmd.CommandText += this._helper.GetOverwriteCommand(propertyname);

                cmd.Connection = conn;
                await this._queue.RunCommand(cmd);
                cmd.Dispose();
                cmd = null;
                GC.Collect();
            }

            DatabaseInserts = null;
            GC.Collect();
        }

        lock (this._bot.guilds)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "guilds", this._bot.guilds.Select(x => new TableDefinitions.guilds
            {
                serverid = x.Key,

                current_locale = x.Value.CurrentLocale,
                override_locale = x.Value.OverrideLocale,

                prefix = x.Value.PrefixSettings.Prefix,
                prefix_disabled = x.Value.PrefixSettings.PrefixDisabled,

                experience_use = x.Value.Experience.UseExperience,
                experience_boost_bumpreminder = x.Value.Experience.BoostXpForBumpReminder,

                auto_assign_role_id = x.Value.Join.AutoAssignRoleId,
                joinlog_channel_id = x.Value.Join.JoinlogChannelId,
                autoban_global_ban = x.Value.Join.AutoBanGlobalBans,
                reapplyroles = x.Value.Join.ReApplyRoles,
                reapplynickname = x.Value.Join.ReApplyNickname,

                tokens_detect = x.Value.TokenLeakDetection.DetectTokens,

                phishing_detect = x.Value.PhishingDetection.DetectPhishing,
                phishing_warnonredirect = x.Value.PhishingDetection.AbuseIpDbReports,
                phishing_abuseipdb = x.Value.PhishingDetection.WarnOnRedirect,
                phishing_type = Convert.ToInt32(x.Value.PhishingDetection.PunishmentType),
                phishing_reason = x.Value.PhishingDetection.CustomPunishmentReason,
                phishing_time = Convert.ToInt64(x.Value.PhishingDetection.CustomPunishmentLength.TotalSeconds),

                bump_enabled = x.Value.BumpReminder.Enabled,
                bump_role = x.Value.BumpReminder.RoleId,
                bump_channel = x.Value.BumpReminder.ChannelId,
                bump_last_reminder = x.Value.BumpReminder.LastReminder.ToUniversalTime().Ticks,
                bump_last_time = x.Value.BumpReminder.LastBump.ToUniversalTime().Ticks,
                bump_last_user = x.Value.BumpReminder.LastUserId,
                bump_message = x.Value.BumpReminder.MessageId,
                bump_persistent_msg = x.Value.BumpReminder.PersistentMessageId,
                bump_missed = x.Value.BumpReminder.BumpsMissed,

                levelrewards = JsonConvert.SerializeObject(x.Value.LevelRewards),
                auditlogcache = JsonConvert.SerializeObject(x.Value.ActionLog.ProcessedAuditLogs),

                crosspostchannels = JsonConvert.SerializeObject(x.Value.Crosspost.CrosspostChannels),
                crosspostdelay = x.Value.Crosspost.DelayBeforePosting,
                crosspostexcludebots = x.Value.Crosspost.ExcludeBots,
                crosspost_ratelimits = JsonConvert.SerializeObject(x.Value.Crosspost.CrosspostRatelimits),

                reactionroles = JsonConvert.SerializeObject(x.Value.ReactionRoles),

                actionlog_channel = x.Value.ActionLog.Channel,
                actionlog_attempt_further_detail = x.Value.ActionLog.AttemptGettingMoreDetails,
                actionlog_log_members_modified = x.Value.ActionLog.MembersModified,
                actionlog_log_member_modified = x.Value.ActionLog.MemberModified,
                actionlog_log_memberprofile_modified = x.Value.ActionLog.MemberProfileModified,
                actionlog_log_message_deleted = x.Value.ActionLog.MessageDeleted,
                actionlog_log_message_updated = x.Value.ActionLog.MessageModified,
                actionlog_log_roles_modified = x.Value.ActionLog.RolesModified,
                actionlog_log_banlist_modified = x.Value.ActionLog.BanlistModified,
                actionlog_log_guild_modified = x.Value.ActionLog.GuildModified,
                actionlog_log_invites_modified = x.Value.ActionLog.InvitesModified,
                actionlog_log_voice_state = x.Value.ActionLog.VoiceStateUpdated,
                actionlog_log_channels_modified = x.Value.ActionLog.ChannelsModified,

                vc_privacy_clear = x.Value.InVoiceTextPrivacy.ClearTextEnabled,
                vc_privacy_perms = x.Value.InVoiceTextPrivacy.SetPermissionsEnabled,

                invitetracker_enabled = x.Value.InviteTracker.Enabled,
                invitetracker_cache = JsonConvert.SerializeObject(x.Value.InviteTracker.Cache),
                invitenotes = JsonConvert.SerializeObject(x.Value.InviteNotes.Notes),

                autounarchivelist = JsonConvert.SerializeObject(x.Value.AutoUnarchiveThreads),

                normalizenames = x.Value.NameNormalizer.NameNormalizerEnabled,

                embed_messages = x.Value.EmbedMessage.UseEmbedding,
                embed_github = x.Value.EmbedMessage.UseGithubEmbedding,

                lavalink_channel = x.Value.MusicModule.ChannelId,
                lavalink_currentposition = x.Value.MusicModule.CurrentVideoPosition,
                lavalink_currentvideo = x.Value.MusicModule.CurrentVideo,
                lavalink_paused = x.Value.MusicModule.IsPaused,
                lavalink_shuffle = x.Value.MusicModule.Shuffle,
                lavalink_repeat = x.Value.MusicModule.Repeat,
                lavalink_queue = JsonConvert.SerializeObject(x.Value.MusicModule.SongQueue),

                polls = JsonConvert.SerializeObject(x.Value.Polls.RunningPolls),

                vccreator_channelid = x.Value.VcCreator.Channel,
                vccreator_channellist = JsonConvert.SerializeObject(x.Value.VcCreator.CreatedChannels),
            }).ToList()));
        }

        lock (this._bot.objectedUsers)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "objected_users", this._bot.objectedUsers.Select(x => new TableDefinitions.objected_users
            {
                id = x
            }).ToList()));
        }

        lock (this._bot.users)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "users", this._bot.users.Select(x => new TableDefinitions.users
            {
                userid = x.Key,
                afk_since = x.Value.AfkStatus.TimeStamp.ToUniversalTime().Ticks,
                afk_reason = x.Value.AfkStatus.Reason,
                afk_pings = JsonConvert.SerializeObject(x.Value.AfkStatus.Messages),
                afk_pingamount = x.Value.AfkStatus.MessagesAmount,
                experience_directmessageoptout = x.Value.ExperienceUser.DirectMessageOptOut,
                submission_accepted_tos = x.Value.UrlSubmissions.AcceptedTOS,
                submission_accepted_submissions = JsonConvert.SerializeObject(x.Value.UrlSubmissions.AcceptedSubmissions),
                playlists = JsonConvert.SerializeObject(x.Value.UserPlaylists),
                reminders = JsonConvert.SerializeObject(x.Value.Reminders.ScheduledReminders),
                submission_last_datetime = x.Value.UrlSubmissions.LastTime.Ticks,
                scoresaber_id = x.Value.ScoreSaber.Id,
                last_google_source = x.Value.Translation.LastGoogleSource,
                last_google_target = x.Value.Translation.LastGoogleTarget,
                last_libretranslate_source = x.Value.Translation.LastLibreTranslateSource,
                last_libretranslate_target = x.Value.Translation.LastLibreTranslateTarget,
                current_locale = x.Value.CurrentLocale,
                override_locale = x.Value.OverrideLocale,
                data_deletion_date = x.Value.Data.DeletionRequestDate.Ticks,
                deletion_requested = x.Value.Data.DeletionRequested,
                last_data_request = x.Value.Data.LastDataRequest.Ticks
            }).ToList()));
        }

        lock (this._bot.phishingUrlSubmissionUserBans)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "submission_user_bans", this._bot.phishingUrlSubmissionUserBans.Select(x => new TableDefinitions.submission_user_bans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator
            }).ToList()));
        }

        lock (this._bot.phishingUrlSubmissionGuildBans)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "submission_guild_bans", this._bot.phishingUrlSubmissionGuildBans.Select(x => new TableDefinitions.submission_guild_bans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator
            }).ToList()));
        }

        lock (this._bot.bannedUsers)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "banned_users", this._bot.bannedUsers.Select(x => new TableDefinitions.banned_users
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }

        lock (this._bot.bannedGuilds)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "banned_guilds", this._bot.bannedGuilds.Select(x => new TableDefinitions.banned_guilds
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }

        lock (this._bot.globalBans)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "globalbans", this._bot.globalBans.Select(x => new TableDefinitions.globalbans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }

        lock (this._bot.globalNotes)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "globalnotes", this._bot.globalNotes.Select(x => new TableDefinitions.globalnotes
            {
                id = x.Key,
                notes = JsonConvert.SerializeObject(x.Value),
            }).ToList()));
        }

        lock (this._bot.submittedUrls)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "active_url_submissions", this._bot.submittedUrls.Select(x => new TableDefinitions.active_url_submissions
            {
                messageid = x.Key,
                url = x.Value.Url,
                submitter = x.Value.Submitter,
                guild = x.Value.GuildOrigin
            }).ToList()));
        }

        var check = CheckGuildTables();
        try
        { check.Add(this._bot); await check.WaitAsync(TimeSpan.FromSeconds(120)); }
        catch { }

        lock (this._bot.guilds)
        {
            foreach (var guild in this._bot.guilds)
                syncs_running.Add(SyncTable(this.guildDatabaseConnection, $"{guild.Key}", guild.Value.Members.Select(x => new TableDefinitions.guild_users
                {
                    userid = x.Key,

                    experience = x.Value.Experience.Points,
                    experience_level = x.Value.Experience.Level,
                    experience_last_message = x.Value.Experience.Last_Message.ToUniversalTime().Ticks,
                    first_join = x.Value.FirstJoinDate.ToUniversalTime().Ticks,
                    last_leave = x.Value.LastLeaveDate.ToUniversalTime().Ticks,
                    roles = JsonConvert.SerializeObject(x.Value.MemberRoles),
                    saved_nickname = x.Value.SavedNickname,
                    invite_code = x.Value.InviteTracker.Code,
                    invite_user = x.Value.InviteTracker.UserId,
                }).ToList(), "guild_users"));
        }

        lock (this._bot.phishingUrls)
        {
            syncs_running.Add(SyncTable(this.mainDatabaseConnection, "scam_urls", this._bot.phishingUrls.Select(x => new TableDefinitions.scam_urls
            {
                url = x.Value.Url,
                origin = JsonConvert.SerializeObject(x.Value.Origin),
                submitter = x.Value.Submitter
            }).ToList()));
        }

        while (syncs_running.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        this.RunningFullSync = false;
        _logger.LogInfo("Full database sync completed.");

        await Task.Delay(1000);
        GC.Collect();
    }

    public async Task UpdateValue(string table, string columnKey, object rowKey, string columnToEdit, object newValue, MySqlConnection connection)
    {
        if (!this._bot.status.DatabaseInitialLoadCompleted)
            return;

        this._queue.RunCommand(new MySqlCommand(this._helper.GetUpdateValueCommand(table, columnKey, rowKey, columnToEdit, newValue), connection), QueuePriority.Low).Add(this._bot);
        return;
    }

    internal async Task Dispose()
    {
        foreach (var b in ScheduledTaskExtensions.GetScheduledTasks().Where(x => x.CustomData?.ToString() == "database-connection-watcher"))
            b.Delete();

        int timeout = 0;

        while (timeout < 30 && this._queue.QueueCount != 0)
        {
            timeout++;
            await Task.Delay(1000);
        }

        this.Disposed = true;

        await this.mainDatabaseConnection.CloseAsync();
    }

    public bool IsDisposed()
    {
        return this.Disposed;
    }
}
