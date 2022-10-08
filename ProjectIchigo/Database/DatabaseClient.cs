using ProjectIchigo.Entities.Database.ColumnAttributes;
using ProjectIchigo.Entities.Database.ColumnTypes;

namespace ProjectIchigo.Database;

internal class DatabaseClient
{
    internal MySqlConnection mainDatabaseConnection { get; set; }
    internal MySqlConnection guildDatabaseConnection { get; set; }
    internal DatabaseHelper _helper { get; private set; }
    internal DatabaseQueue _queue { get; private set; }

    public Bot _bot { private get; set; }

    private bool Disposed { get; set; } = false;

    public static async Task<DatabaseClient> InitializeDatabase(Bot _bot)
    {
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
                    _logger.LogWarn($"Missing table '{b.Name}'. Creating..");
                    string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.MainDatabaseName}`.`{b.Name}` ( {string.Join(", ", b.GetProperties().Select(x => $"`{x.Name}` {x.PropertyType.Name.ToUpper()}{(x.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : "")}{(x.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(x.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out _) ? " NULL" : " NOT NULL")}"))}{(b.GetProperties().Any(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)) ? $", PRIMARY KEY (`{b.GetProperties().First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")})";

                    var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = databaseClient.mainDatabaseConnection;

                    await databaseClient._queue.RunCommand(cmd);
                    _logger.LogInfo($"Created table '{b.Name}'.");
                }

                var Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);

                foreach (var col in b.GetProperties())
                {
                    if (!Columns.ContainsKey(col.Name.ToLower()))
                    {
                        _logger.LogWarn($"Missing column '{col.Name}' in '{b.Name}'. Creating..");
                        string sql = $"ALTER TABLE `{b.Name}` ADD `{col.Name}` {col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue1) ? $"({maxvalue1.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo($"Created column '{col.Name}' in '{b.Name}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);
                    }

                    if (Columns[col.Name].ToLower() != col.PropertyType.Name.ToLower() + (col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
                    {
                        _logger.LogWarn($"Wrong data type for column '{col.Name}' in '{b.Name}'");
                        string sql = $"ALTER TABLE `{b.Name}` CHANGE `{col.Name}` `{col.Name}` {col.PropertyType.Name.ToUpper()}{(maxvalue is not null ? $"({maxvalue.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo($"Changed column '{col.Name}' in '{b.Name}' to datatype '{col.PropertyType.Name.ToUpper()}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Name);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!b.GetProperties().Any(x => x.Name == col.Key))
                    {
                        _logger.LogWarn($"Invalid column '{col.Key}' in '{b.Name}'");

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
        })).CreateScheduleTask(DateTime.UtcNow.AddSeconds(10), "database-connection-watcher");

        return databaseClient;
    }

    public async Task CheckGuildTables()
    {
        while (_queue.QueueCount!= 0)
            await Task.Delay(500);

        IEnumerable<string> GuildTables;

        int retries = 1;

        while (true)
        {
            try
            {
                GuildTables = await _helper.ListTables(guildDatabaseConnection);
                break;
            }
            catch (Exception ex)
            {
                if (retries >= 3)
                {
                    throw;
                }

                _logger.LogWarn($"Failed to get a list of guild tables. Retrying in 1000ms.. ({retries}/3)", ex);
                retries++;
                await Task.Delay(1000);
            }
        }

        foreach (var b in _bot.guilds)
        {
            if (!GuildTables.Contains($"{b.Key}"))
            {
                _logger.LogWarn($"Missing table '{b.Key}'. Creating..");
                string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", typeof(TableDefinitions.guild_users).GetProperties().Select(x => $"`{x.Name}` {x.PropertyType.Name.ToUpper()}{(x.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : "")}{(x.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(x.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out _) ? " NULL" : " NOT NULL")}"))}{(typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)) ? $", PRIMARY KEY (`{typeof(TableDefinitions.guild_users).GetProperties().First(x => x.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _)).Name}`)" : "")})";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);
                _logger.LogInfo($"Created table '{b.Key}'.");
            }
        }

        GuildTables = await _helper.ListTables(guildDatabaseConnection);

        foreach (var b in GuildTables)
        {
            if (b != "writetester")
            {
                var Columns = await _helper.ListColumns(guildDatabaseConnection, b);

                foreach (var col in typeof(TableDefinitions.guild_users).GetProperties())
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        _logger.LogWarn($"Missing column '{col.Name}' in '{b}'. Creating..");
                        string sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue1) ? $"({maxvalue1.MaxValue})" : "")}{col.PropertyType.Name.ToUpper()}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        _logger.LogInfo($"Created column '{col.Name}' in '{b}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }

                    if (Columns[col.Name].ToLower() != col.PropertyType.Name.ToLower() + (col.TryGetCustomAttribute<MaxValueAttribute>(typeof(MaxValueAttribute), out var maxvalue) ? $"({maxvalue.MaxValue})" : ""))
                    {
                        _logger.LogWarn($"Wrong data type for column '{col.Name}' in '{b}'");
                        string sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.PropertyType.Name.ToUpper()}{(maxvalue is not null ? $"({maxvalue.MaxValue})" : "")}{(col.TryGetCustomAttribute<CollationAttribute>(typeof(CollationAttribute), out var collation) ? $" CHARACTER SET {collation.Collation[..collation.Collation.IndexOf("_")]} COLLATE {collation.Collation}" : "")}{(col.TryGetCustomAttribute<NullableAttribute>(typeof(NullableAttribute), out var nullable) ? " NULL" : " NOT NULL")}{(nullable is not null && col.TryGetCustomAttribute<DefaultAttribute>(typeof(DefaultAttribute), out var defaultv) ? $" DEFAULT '{defaultv.Default}'" : "")}{(col.TryGetCustomAttribute<PrimaryAttribute>(typeof(PrimaryAttribute), out _) ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        _logger.LogInfo($"Changed column '{col.Name}' in '{b}' to datatype '{col.PropertyType.Name.ToUpper()}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!typeof(TableDefinitions.guild_users).GetProperties().Any(x => x.Name == col.Key))
                    {
                        _logger.LogWarn($"Invalid column '{col.Key}' in '{b}'");

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{b}` DROP COLUMN `{col.Key}`";
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);
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
        })).CreateScheduleTask(DateTime.UtcNow.AddSeconds(120), "database-connection-watcher");

        if (Disposed)
            return;

        while (_queue.QueueCount > 0)
            await Task.Delay(100);

        if (!await _queue.RunPing(connection))
        {
            try
            {
                _logger.LogWarn("Pinging the database failed, attempting reconnect.");
                connection.Open();
                _logger.LogInfo($"Reconnected to database.");
            }
            catch (Exception ex)
            {
                _logger.LogFatal($"Reconnecting to the database failed. Cannot sync changes to database", ex);
                return;
            }
        }

        try
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = _helper.GetSaveCommand("writetester");

            cmd.CommandText += _helper.GetValueCommand("writetester", 1);

            cmd.Parameters.AddWithValue($"aaa1", 1);

            cmd.CommandText = cmd.CommandText[..(cmd.CommandText.Length - 2)];
            cmd.CommandText += _helper.GetOverwriteCommand("writetester");

            cmd.Connection = connection;
            await _queue.RunCommand(cmd);

            await _helper.DeleteRow(connection, "writetester", "aaa", "1");
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogWarn($"Creating a test value in database failed, reconnecting to database", ex);
                connection.Close();
                connection.Open();
                _logger.LogInfo($"Reconnected to database.");
            }
            catch (Exception ex1)
            {
                _logger.LogFatal($"Reconnecting to the database failed. Cannot sync changes to database", ex1);
                return;
            }
        }
    }

    public bool RunningFullSync = false;
    public CancellationTokenSource FullSyncCancel = new();
    public DateTimeOffset LastFullSync = DateTimeOffset.MinValue;

    public async Task FullSyncDatabase(bool Important = false)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (Important && RunningFullSync)
        {
            FullSyncCancel.Cancel();
            while (RunningFullSync)
                await Task.Delay(100);
        }

        if (!Important && LastFullSync.GetTimespanSince() < TimeSpan.FromMinutes(20))
            return;

        LastFullSync = DateTimeOffset.UtcNow;

        if (RunningFullSync)
            return;

        bool IsCancellationRequested()
        {
            if (FullSyncCancel.IsCancellationRequested)
            {
                RunningFullSync = false;
                FullSyncCancel = new();
                return true;
            }

            return false;
        }

        RunningFullSync = true;

        _logger.LogDebug("Running full database sync..");

        if (mainDatabaseConnection == null || guildDatabaseConnection == null)
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
                _logger.LogDebug($"Writing to table {table}/{propertyname} with {chunk.Length} inserts");

                var cmd = conn.CreateCommand();
                cmd.CommandText = _helper.GetSaveCommand(table, propertyname);

                for (int i = 0; i < chunk.Length; i++)
                {
                    var b = chunk[i];
                    var properties = b.GetType().GetProperties();

                    cmd.CommandText += _helper.GetValueCommand(propertyname, i);
                    for (int i1 = 0; i1 < properties.Length; i1++)
                    {
                        var prop = properties[i1];

                        cmd.Parameters.AddWithValue($"{prop.Name}{i}", ((BaseColumn)prop.GetValue(b)).GetValue());
                    }
                    properties = null;
                }

                cmd.CommandText = cmd.CommandText[..(cmd.CommandText.Length - 2)];
                cmd.CommandText += _helper.GetOverwriteCommand(propertyname);

                cmd.Connection = conn;
                await _queue.RunCommand(cmd);
                cmd.Dispose();
                cmd = null;
                GC.Collect();
            }

            DatabaseInserts = null;
            GC.Collect();
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "guilds", _bot.guilds.Select(x => new TableDefinitions.guilds
            {
                serverid = x.Key,

                experience_use = x.Value.ExperienceSettings.UseExperience,
                experience_boost_bumpreminder = x.Value.ExperienceSettings.BoostXpForBumpReminder,

                auto_assign_role_id = x.Value.JoinSettings.AutoAssignRoleId,
                joinlog_channel_id = x.Value.JoinSettings.JoinlogChannelId,
                autoban_global_ban = x.Value.JoinSettings.AutoBanGlobalBans,
                reapplyroles = x.Value.JoinSettings.ReApplyRoles,
                reapplynickname = x.Value.JoinSettings.ReApplyNickname,

                tokens_detect = x.Value.TokenLeakDetectionSettings.DetectTokens,

                phishing_detect = x.Value.PhishingDetectionSettings.DetectPhishing,
                phishing_warnonredirect = x.Value.PhishingDetectionSettings.AbuseIpDbReports,
                phishing_abuseipdb = x.Value.PhishingDetectionSettings.WarnOnRedirect,
                phishing_type = Convert.ToInt32(x.Value.PhishingDetectionSettings.PunishmentType),
                phishing_reason = x.Value.PhishingDetectionSettings.CustomPunishmentReason,
                phishing_time = Convert.ToInt64(x.Value.PhishingDetectionSettings.CustomPunishmentLength.TotalSeconds),

                bump_enabled = x.Value.BumpReminderSettings.Enabled,
                bump_role = x.Value.BumpReminderSettings.RoleId,
                bump_channel = x.Value.BumpReminderSettings.ChannelId,
                bump_last_reminder = x.Value.BumpReminderSettings.LastReminder.ToUniversalTime().Ticks,
                bump_last_time = x.Value.BumpReminderSettings.LastBump.ToUniversalTime().Ticks,
                bump_last_user = x.Value.BumpReminderSettings.LastUserId,
                bump_message = x.Value.BumpReminderSettings.MessageId,
                bump_persistent_msg = x.Value.BumpReminderSettings.PersistentMessageId,
                bump_missed = x.Value.BumpReminderSettings.BumpsMissed,

                levelrewards = JsonConvert.SerializeObject(x.Value.LevelRewards),
                auditlogcache = JsonConvert.SerializeObject(x.Value.ProcessedAuditLogs),

                crosspostchannels = JsonConvert.SerializeObject(x.Value.CrosspostSettings.CrosspostChannels),
                crosspostdelay = x.Value.CrosspostSettings.DelayBeforePosting,
                crosspostexcludebots = x.Value.CrosspostSettings.ExcludeBots,
                crosspost_ratelimits = JsonConvert.SerializeObject(x.Value.CrosspostSettings.CrosspostRatelimits),

                reactionroles = JsonConvert.SerializeObject(x.Value.ReactionRoles),

                actionlog_channel = x.Value.ActionLogSettings.Channel,
                actionlog_attempt_further_detail = x.Value.ActionLogSettings.AttemptGettingMoreDetails,
                actionlog_log_members_modified = x.Value.ActionLogSettings.MembersModified,
                actionlog_log_member_modified = x.Value.ActionLogSettings.MemberModified,
                actionlog_log_memberprofile_modified = x.Value.ActionLogSettings.MemberProfileModified,
                actionlog_log_message_deleted = x.Value.ActionLogSettings.MessageDeleted,
                actionlog_log_message_updated = x.Value.ActionLogSettings.MessageModified,
                actionlog_log_roles_modified = x.Value.ActionLogSettings.RolesModified,
                actionlog_log_banlist_modified = x.Value.ActionLogSettings.BanlistModified,
                actionlog_log_guild_modified = x.Value.ActionLogSettings.GuildModified,
                actionlog_log_invites_modified = x.Value.ActionLogSettings.InvitesModified,
                actionlog_log_voice_state = x.Value.ActionLogSettings.VoiceStateUpdated,
                actionlog_log_channels_modified = x.Value.ActionLogSettings.ChannelsModified,

                vc_privacy_clear = x.Value.InVoiceTextPrivacySettings.ClearTextEnabled,
                vc_privacy_perms = x.Value.InVoiceTextPrivacySettings.SetPermissionsEnabled,

                invitetracker_enabled = x.Value.InviteTrackerSettings.Enabled,
                invitetracker_cache = JsonConvert.SerializeObject(x.Value.InviteTrackerSettings.Cache),

                autounarchivelist = JsonConvert.SerializeObject(x.Value.AutoUnarchiveThreads),

                normalizenames = x.Value.NameNormalizerSettings.NameNormalizerEnabled,

                embed_messages = x.Value.EmbedMessageSettings.UseEmbedding,
                embed_github = x.Value.EmbedMessageSettings.UseGithubEmbedding,

                lavalink_channel = x.Value.Lavalink.ChannelId,
                lavalink_currentposition = x.Value.Lavalink.CurrentVideoPosition,
                lavalink_currentvideo = x.Value.Lavalink.CurrentVideo,
                lavalink_paused = x.Value.Lavalink.IsPaused,
                lavalink_shuffle = x.Value.Lavalink.Shuffle,
                lavalink_repeat = x.Value.Lavalink.Repeat,
                lavalink_queue = JsonConvert.SerializeObject(x.Value.Lavalink.SongQueue),
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the guilds table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "objected_users", _bot.objectedUsers.Select(x => new TableDefinitions.objected_users
            {
                id = x
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the objected_users table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "users", _bot.users.Select(x => new TableDefinitions.users
            {
                userid = x.Key,
                afk_since = x.Value.AfkStatus.TimeStamp.ToUniversalTime().Ticks,
                afk_reason = x.Value.AfkStatus.Reason,
                afk_pings = JsonConvert.SerializeObject(x.Value.AfkStatus.Messages),
                afk_pingamount = x.Value.AfkStatus.MessagesAmount,
                experience_directmessageoptout = x.Value.ExperienceUserSettings.DirectMessageOptOut,
                submission_accepted_tos = x.Value.UrlSubmissions.AcceptedTOS,
                submission_accepted_submissions = JsonConvert.SerializeObject(x.Value.UrlSubmissions.AcceptedSubmissions),
                playlists = JsonConvert.SerializeObject(x.Value.UserPlaylists),
                reminders = JsonConvert.SerializeObject(x.Value.ReminderSettings.ScheduledReminders),
                submission_last_datetime = x.Value.UrlSubmissions.LastTime.Ticks,
                scoresaber_id = x.Value.ScoreSaber.Id
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the users table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "submission_user_bans", _bot.phishingUrlSubmissionUserBans.Select(x => new TableDefinitions.submission_user_bans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the submission_user_bans table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "submission_guild_bans", _bot.phishingUrlSubmissionGuildBans.Select(x => new TableDefinitions.submission_guild_bans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the submission_guild_bans table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "banned_users", _bot.bannedUsers.Select(x => new TableDefinitions.banned_users
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the banned_users table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "banned_guilds", _bot.bannedGuilds.Select(x => new TableDefinitions.banned_guilds
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the banned_guilds table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "globalbans", _bot.globalBans.Select(x => new TableDefinitions.globalbans
            {
                id = x.Key,
                reason = x.Value.Reason,
                moderator = x.Value.Moderator,
                timestamp = x.Value.Timestamp.Ticks
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the submission_guild_bans table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "globalnotes", _bot.globalNotes.Select(x => new TableDefinitions.globalnotes
            {
                id = x.Key,
                notes = JsonConvert.SerializeObject(x.Value),
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the submission_guild_bans table", ex);
        }

        try
        {
            syncs_running.Add(SyncTable(mainDatabaseConnection, "active_url_submissions", _bot.submittedUrls.Select(x => new TableDefinitions.active_url_submissions
            {
                messageid = x.Key,
                url = x.Value.Url,
                submitter = x.Value.Submitter,
                guild = x.Value.GuildOrigin
            }).ToList()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occurred while trying to update the active_url_submissions table", ex);
        }

        var check = CheckGuildTables();
        try { check.Add(_bot.watcher); await check.WaitAsync(TimeSpan.FromSeconds(120)); } catch { }

        foreach (var guild in _bot.guilds.ToList())
            try
            {
                syncs_running.Add(SyncTable(guildDatabaseConnection, $"{guild.Key}", guild.Value.Members.Select(x => new TableDefinitions.guild_users
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
            catch (Exception ex)
            {
                _logger.LogError($"An exception occurred while trying to update the {guild.Key} table", ex);
            }

        syncs_running.Add(SyncTable(mainDatabaseConnection, "scam_urls", _bot.phishingUrls.Select(x => new TableDefinitions.scam_urls
        {
            url = x.Value.Url,
            origin = JsonConvert.SerializeObject(x.Value.Origin),
            submitter = x.Value.Submitter
        }).ToList()));

        while (syncs_running.Any(x => !x.IsCompleted))
            await Task.Delay(100);

        RunningFullSync = false;
        _logger.LogInfo("Full database sync completed.");

        await Task.Delay(1000);
        GC.Collect();
    }

    public async Task UpdateValue(string table, string columnKey, object rowKey, string columnToEdit, object newValue, MySqlConnection connection)
    {
        if (!_bot.status.DatabaseInitialLoadCompleted)
            return;

        _queue.RunCommand(new MySqlCommand(_helper.GetUpdateValueCommand(table, columnKey, rowKey, columnToEdit, newValue), connection), QueuePriority.Low).Add(_bot.watcher);
        return;
    }

    public async Task Dispose()
    {
        foreach (var b in GetScheduleTasks())
            if (b.Value.customId == "database-connection-watcher")
                DeleteScheduleTask(b.Key);

        int timeout = 0;

        while (timeout < 30 && _queue.QueueCount!= 0)
        {
            timeout++;
            await Task.Delay(1000);
        }

        Disposed = true;

        await mainDatabaseConnection.CloseAsync();
    }

    public bool IsDisposed()
    {
        return Disposed;
    }
}
