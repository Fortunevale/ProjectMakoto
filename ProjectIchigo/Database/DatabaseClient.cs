namespace ProjectIchigo.Database;

internal class DatabaseClient
{
    internal MySqlConnection mainDatabaseConnection { get; set; }
    internal MySqlConnection guildDatabaseConnection { get; set; }
    internal DatabaseHelper _helper { get; private set; }
    internal DatabaseQueue _queue { get; private set; }

    public Bot _bot { private get; set; }

    private bool Disposed { get; set; } = false;

    private Dictionary<Task, bool> queuedUpdates = new();

    public async Task QueueWatcher()
    {
        CancellationTokenSource tokenSource = new();

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(30));

                if (Disposed)
                    return;

                _ = FullSyncDatabase();
            }
        });

        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (Disposed)
                    return;

                if (queuedUpdates.Any(x => x.Value))
                {
                    foreach (var b in queuedUpdates.Where(x => !x.Value).ToList())
                        queuedUpdates.Remove(b.Key);

                    tokenSource.Cancel();
                    tokenSource = new();
                }

                Thread.Sleep(100);
            }
        });

        while (true)
        {
            if (Disposed)
                return;

            try
            {
                if (queuedUpdates.Any(x => x.Key.IsCompleted))
                    foreach (var task in queuedUpdates.Where(x => x.Key.IsCompleted).ToList())
                        queuedUpdates.Remove(task.Key);

                foreach (var task in queuedUpdates.Where(x => x.Key.Status == TaskStatus.Created).ToList())
                {
                    task.Key.Start();
                    await Task.Delay(20000, tokenSource.Token);
                }

                await Task.Delay(1000);
            }
            catch (TaskCanceledException)
            {
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred in the database queue handler", ex);
                throw;
            }
        }
    }

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

            foreach (var b in DatabaseColumnLists.Tables)
            {
                if (!MainTables.Contains(b.Key))
                {
                    _logger.LogWarn($"Missing table '{b.Key}'. Creating..");
                    string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.MainDatabaseName}`.`{b.Key}` ( {string.Join(", ", b.Value.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(b.Value.Any(x => x.Primary) ? $", PRIMARY KEY (`{b.Value.First(x => x.Primary).Name}`)" : "")})";

                    var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = databaseClient.mainDatabaseConnection;

                    await databaseClient._queue.RunCommand(cmd);
                    _logger.LogInfo($"Created table '{b.Key}'.");
                }

                var Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);

                foreach (var col in b.Value)
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        _logger.LogWarn($"Missing column '{col.Name}' in '{b.Key}'. Creating..");
                        string sql = $"ALTER TABLE `{b.Key}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo($"Created column '{col.Name}' in '{b.Key}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        _logger.LogWarn($"Wrong data type for column '{col.Name}' in '{b.Key}'");
                        string sql = $"ALTER TABLE `{b.Key}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        _logger.LogInfo($"Changed column '{col.Name}' in '{b.Key}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!b.Value.Any(x => x.Name == col.Key))
                    {
                        _logger.LogWarn($"Invalid column '{col.Key}' in '{b.Key}'");

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{b.Key}` DROP COLUMN `{col.Key}`";
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
                string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", DatabaseColumnLists.guild_users.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.guild_users.Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.guild_users.First(x => x.Primary).Name}`)" : "")})";

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

                foreach (var col in DatabaseColumnLists.guild_users)
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        _logger.LogWarn($"Missing column '{col.Name}' in '{b}'. Creating..");
                        string sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        _logger.LogInfo($"Created column '{col.Name}' in '{b}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        _logger.LogWarn($"Wrong data type for column '{col.Name}' in '{b}'");
                        string sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        _logger.LogInfo($"Changed column '{col.Name}' in '{b}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!DatabaseColumnLists.guild_users.Any(x => x.Name == col.Key))
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

        if (!GuildTables.Contains("writetester"))
        {
            _logger.LogWarn($"Missing table 'writetester'. Creating..");
            string sql = $"CREATE TABLE `{_bot.status.LoadedConfig.Secrets.Database.GuildDatabaseName}`.`writetester` ( {string.Join(", ", DatabaseColumnLists.Tables["writetester"].Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.Tables["writetester"].Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.Tables["writetester"].First(x => x.Primary).Name}`)" : "")})";

            var cmd = guildDatabaseConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = guildDatabaseConnection;

            await _queue.RunCommand(cmd);
            _logger.LogInfo($"Created table 'writetester'.");
        }

        var GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");

        foreach (var col in DatabaseColumnLists.Tables["writetester"])
        {
            if (!GuildColumns.ContainsKey(col.Name))
            {
                _logger.LogWarn($"Missing column '{col.Name}' in 'writetester'. Creating..");
                string sql = $"ALTER TABLE `writetester` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);

                _logger.LogInfo($"Created column '{col.Name}' in 'writetester'.");
                GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");
            }

            if (GuildColumns[col.Name].ToLower() != col.Type.ToLower())
            {
                _logger.LogWarn($"Wrong data type for column '{col.Name}' in 'writetester'");
                string sql = $"ALTER TABLE `writetester` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);

                _logger.LogInfo($"Changed column '{col.Name}' in 'writetester' to datatype '{col.Type.ToUpper()}'.");
                GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");
            }
        }

        foreach (var col in GuildColumns)
        {
            if (!DatabaseColumnLists.Tables["writetester"].Any(x => x.Name == col.Key))
            {
                _logger.LogWarn($"Invalid column '{col.Key}' in 'writetester'");

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = $"ALTER TABLE `writetester` DROP COLUMN `{col.Key}`";
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);
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
            cmd.CommandText = _helper.GetSaveCommand("writetester", DatabaseColumnLists.writetester);

            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.writetester, 1);

            cmd.Parameters.AddWithValue($"aaa1", 1);

            cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
            cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.writetester);

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

    public async Task FullSyncDatabase(bool Important = false)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (queuedUpdates.Count < 2 || Important)
        {
            Task key = new(async () =>
            {
                _logger.LogInfo("Running full Database Sync..");

                if (_bot.guilds.Count > 0)
                    try
                    {
                        List<DatabaseGuildSettings> DatabaseInserts = _bot.guilds.Select(x => new DatabaseGuildSettings
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
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update guilds in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("guilds", DatabaseColumnLists.guilds);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.guilds, i);

                            cmd.Parameters.AddWithValue($"serverid{i}", DatabaseInserts[i].serverid);

                            cmd.Parameters.AddWithValue($"experience_use{i}", DatabaseInserts[i].experience_use);
                            cmd.Parameters.AddWithValue($"experience_boost_bumpreminder{i}", DatabaseInserts[i].experience_boost_bumpreminder);

                            cmd.Parameters.AddWithValue($"auto_assign_role_id{i}", DatabaseInserts[i].auto_assign_role_id);
                            cmd.Parameters.AddWithValue($"joinlog_channel_id{i}", DatabaseInserts[i].joinlog_channel_id);
                            cmd.Parameters.AddWithValue($"autoban_global_ban{i}", DatabaseInserts[i].autoban_global_ban);

                            cmd.Parameters.AddWithValue($"reactionroles{i}", DatabaseInserts[i].reactionroles);
                            cmd.Parameters.AddWithValue($"levelrewards{i}", DatabaseInserts[i].levelrewards);
                            cmd.Parameters.AddWithValue($"auditlogcache{i}", DatabaseInserts[i].auditlogcache);

                            cmd.Parameters.AddWithValue($"crosspostdelay{i}", DatabaseInserts[i].crosspostdelay);
                            cmd.Parameters.AddWithValue($"crosspostchannels{i}", DatabaseInserts[i].crosspostchannels);
                            cmd.Parameters.AddWithValue($"crosspostexcludebots{i}", DatabaseInserts[i].crosspostexcludebots);
                            cmd.Parameters.AddWithValue($"crosspost_ratelimits{i}", DatabaseInserts[i].crosspost_ratelimits);

                            cmd.Parameters.AddWithValue($"reapplyroles{i}", DatabaseInserts[i].reapplyroles);
                            cmd.Parameters.AddWithValue($"reapplynickname{i}", DatabaseInserts[i].reapplynickname);

                            cmd.Parameters.AddWithValue($"bump_enabled{i}", DatabaseInserts[i].bump_enabled);
                            cmd.Parameters.AddWithValue($"bump_role{i}", DatabaseInserts[i].bump_role);
                            cmd.Parameters.AddWithValue($"bump_channel{i}", DatabaseInserts[i].bump_channel);
                            cmd.Parameters.AddWithValue($"bump_last_reminder{i}", DatabaseInserts[i].bump_last_reminder);
                            cmd.Parameters.AddWithValue($"bump_last_time{i}", DatabaseInserts[i].bump_last_time);
                            cmd.Parameters.AddWithValue($"bump_last_user{i}", DatabaseInserts[i].bump_last_user);
                            cmd.Parameters.AddWithValue($"bump_message{i}", DatabaseInserts[i].bump_message);
                            cmd.Parameters.AddWithValue($"bump_persistent_msg{i}", DatabaseInserts[i].bump_persistent_msg);
                            cmd.Parameters.AddWithValue($"bump_missed{i}", DatabaseInserts[i].bump_missed);

                            cmd.Parameters.AddWithValue($"tokens_detect{i}", DatabaseInserts[i].tokens_detect);

                            cmd.Parameters.AddWithValue($"phishing_detect{i}", DatabaseInserts[i].phishing_detect);
                            cmd.Parameters.AddWithValue($"phishing_warnonredirect{i}", DatabaseInserts[i].phishing_warnonredirect);
                            cmd.Parameters.AddWithValue($"phishing_abuseipdb{i}", DatabaseInserts[i].phishing_abuseipdb);
                            cmd.Parameters.AddWithValue($"phishing_type{i}", DatabaseInserts[i].phishing_type);
                            cmd.Parameters.AddWithValue($"phishing_reason{i}", DatabaseInserts[i].phishing_reason);
                            cmd.Parameters.AddWithValue($"phishing_time{i}", DatabaseInserts[i].phishing_time);

                            cmd.Parameters.AddWithValue($"actionlog_channel{i}", DatabaseInserts[i].actionlog_channel);
                            cmd.Parameters.AddWithValue($"actionlog_attempt_further_detail{i}", DatabaseInserts[i].actionlog_attempt_further_detail);
                            cmd.Parameters.AddWithValue($"actionlog_log_members_modified{i}", DatabaseInserts[i].actionlog_log_members_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_member_modified{i}", DatabaseInserts[i].actionlog_log_member_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_memberprofile_modified{i}", DatabaseInserts[i].actionlog_log_memberprofile_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_message_deleted{i}", DatabaseInserts[i].actionlog_log_message_deleted);
                            cmd.Parameters.AddWithValue($"actionlog_log_message_updated{i}", DatabaseInserts[i].actionlog_log_message_updated);
                            cmd.Parameters.AddWithValue($"actionlog_log_roles_modified{i}", DatabaseInserts[i].actionlog_log_roles_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_banlist_modified{i}", DatabaseInserts[i].actionlog_log_banlist_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_guild_modified{i}", DatabaseInserts[i].actionlog_log_guild_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_channels_modified{i}", DatabaseInserts[i].actionlog_log_channels_modified);
                            cmd.Parameters.AddWithValue($"actionlog_log_voice_state{i}", DatabaseInserts[i].actionlog_log_voice_state);
                            cmd.Parameters.AddWithValue($"actionlog_log_invites_modified{i}", DatabaseInserts[i].actionlog_log_invites_modified);

                            cmd.Parameters.AddWithValue($"vc_privacy_clear{i}", DatabaseInserts[i].vc_privacy_clear);
                            cmd.Parameters.AddWithValue($"vc_privacy_perms{i}", DatabaseInserts[i].vc_privacy_perms);

                            cmd.Parameters.AddWithValue($"invitetracker_enabled{i}", DatabaseInserts[i].invitetracker_enabled);
                            cmd.Parameters.AddWithValue($"invitetracker_cache{i}", DatabaseInserts[i].invitetracker_cache);

                            cmd.Parameters.AddWithValue($"autounarchivelist{i}", DatabaseInserts[i].autounarchivelist);

                            cmd.Parameters.AddWithValue($"normalizenames{i}", DatabaseInserts[i].normalizenames);

                            cmd.Parameters.AddWithValue($"embed_messages{i}", DatabaseInserts[i].embed_messages);
                            cmd.Parameters.AddWithValue($"embed_github{i}", DatabaseInserts[i].embed_github);

                            cmd.Parameters.AddWithValue($"lavalink_channel{i}", DatabaseInserts[i].lavalink_channel);
                            cmd.Parameters.AddWithValue($"lavalink_currentvideo{i}", DatabaseInserts[i].lavalink_currentvideo);
                            cmd.Parameters.AddWithValue($"lavalink_currentposition{i}", DatabaseInserts[i].lavalink_currentposition);
                            cmd.Parameters.AddWithValue($"lavalink_paused{i}", DatabaseInserts[i].lavalink_paused);
                            cmd.Parameters.AddWithValue($"lavalink_shuffle{i}", DatabaseInserts[i].lavalink_shuffle);
                            cmd.Parameters.AddWithValue($"lavalink_repeat{i}", DatabaseInserts[i].lavalink_repeat);
                            cmd.Parameters.AddWithValue($"lavalink_queue{i}", DatabaseInserts[i].lavalink_queue);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guilds);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the guilds table", ex);
                    }

                var check = CheckGuildTables();
                check.Add(_bot.watcher);

                if (_bot.guilds.Count > 0)
                    foreach (var guild in _bot.guilds.ToList())
                        if (guild.Value.Members.Count > 0)
                        {
                            try
                            {
                                List<DatabaseMembers> DatabaseInserts = guild.Value.Members.Select(x => new DatabaseMembers
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
                                }).ToList();

                                if (mainDatabaseConnection == null)
                                {
                                    throw new Exception($"Exception occurred while trying to update guilds in database: Database mainDatabaseConnection not present");
                                }

                                var cmd = mainDatabaseConnection.CreateCommand();
                                cmd.CommandText = _helper.GetSaveCommand($"{guild.Key}", DatabaseColumnLists.guild_users);

                                for (int i = 0; i < DatabaseInserts.Count; i++)
                                {
                                    cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.guild_users, i);

                                    cmd.Parameters.AddWithValue($"userid{i}", DatabaseInserts[i].userid);

                                    cmd.Parameters.AddWithValue($"experience{i}", DatabaseInserts[i].experience);
                                    cmd.Parameters.AddWithValue($"experience_level{i}", DatabaseInserts[i].experience_level);
                                    cmd.Parameters.AddWithValue($"experience_last_message{i}", DatabaseInserts[i].experience_last_message);
                                    cmd.Parameters.AddWithValue($"first_join{i}", DatabaseInserts[i].first_join);
                                    cmd.Parameters.AddWithValue($"last_leave{i}", DatabaseInserts[i].last_leave);
                                    cmd.Parameters.AddWithValue($"roles{i}", DatabaseInserts[i].roles);
                                    cmd.Parameters.AddWithValue($"saved_nickname{i}", DatabaseInserts[i].saved_nickname);
                                    cmd.Parameters.AddWithValue($"invite_code{i}", DatabaseInserts[i].invite_code);
                                    cmd.Parameters.AddWithValue($"invite_user{i}", DatabaseInserts[i].invite_user);
                                }

                                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                                cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guild_users);

                                cmd.Connection = guildDatabaseConnection;
                                await _queue.RunCommand(cmd);

                                DatabaseInserts.Clear();
                                DatabaseInserts = null;
                                cmd.Dispose();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"An exception occurred while trying to update the {guild.Key} table", ex);
                            }
                        }

                if (_bot.objectedUsers.Count > 0)
                    try
                    {
                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to objected users in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand($"objected_users", DatabaseColumnLists.objected_users);

                        for (int i = 0; i < _bot.objectedUsers.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.objected_users, i);

                            cmd.Parameters.AddWithValue($"id{i}", _bot.objectedUsers[i]);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.objected_users);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the objected_users table", ex);
                    }

                if (_bot.users.Count > 0)
                    try
                    {
                        List<DatabaseUsers> DatabaseInserts = _bot.users.Select(x => new DatabaseUsers
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
                            submission_last_datetime = x.Value.UrlSubmissions.LastTime.Ticks,
                            scoresaber_id = x.Value.ScoreSaber.Id
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update users in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = cmd.CommandText = _helper.GetSaveCommand("users", DatabaseColumnLists.users);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.users, i);

                            cmd.Parameters.AddWithValue($"userid{i}", DatabaseInserts[i].userid);
                            cmd.Parameters.AddWithValue($"scoresaber_id{i}", DatabaseInserts[i].scoresaber_id);
                            cmd.Parameters.AddWithValue($"afk_since{i}", DatabaseInserts[i].afk_since);
                            cmd.Parameters.AddWithValue($"afk_reason{i}", DatabaseInserts[i].afk_reason);
                            cmd.Parameters.AddWithValue($"afk_pings{i}", DatabaseInserts[i].afk_pings);
                            cmd.Parameters.AddWithValue($"afk_pingamount{i}", DatabaseInserts[i].afk_pingamount);
                            cmd.Parameters.AddWithValue($"experience_directmessageoptout{i}", DatabaseInserts[i].experience_directmessageoptout);
                            cmd.Parameters.AddWithValue($"submission_accepted_tos{i}", DatabaseInserts[i].submission_accepted_tos);
                            cmd.Parameters.AddWithValue($"submission_accepted_submissions{i}", DatabaseInserts[i].submission_accepted_submissions);
                            cmd.Parameters.AddWithValue($"submission_last_datetime{i}", DatabaseInserts[i].submission_last_datetime);
                            cmd.Parameters.AddWithValue($"playlists{i}", DatabaseInserts[i].playlists);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.users);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the users table", ex);
                    }

                if (_bot.phishingUrlSubmissionUserBans.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot.phishingUrlSubmissionUserBans.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update submission_user_bans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("submission_user_bans", DatabaseColumnLists.submission_user_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.submission_user_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.submission_user_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the submission_user_bans table", ex);
                    }

                if (_bot.phishingUrlSubmissionGuildBans.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot.phishingUrlSubmissionGuildBans.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update submission_guild_bans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("submission_guild_bans", DatabaseColumnLists.submission_guild_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.submission_guild_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.submission_guild_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the submission_guild_bans table", ex);
                    }

                if (_bot.bannedUsers.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot.bannedUsers.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator,
                            timestamp = x.Value.Timestamp.Ticks
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update banned users in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("banned_users", DatabaseColumnLists.globalbans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.globalbans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                            cmd.Parameters.AddWithValue($"timestamp{i}", DatabaseInserts[i].timestamp);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.globalbans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the banned_users table", ex);
                    }

                if (_bot.bannedGuilds.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot.bannedGuilds.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator,
                            timestamp = x.Value.Timestamp.Ticks
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update banned guilds in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("banned_guilds", DatabaseColumnLists.globalbans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.globalbans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                            cmd.Parameters.AddWithValue($"timestamp{i}", DatabaseInserts[i].timestamp);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.globalbans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the banned_guilds table", ex);
                    }

                if (_bot.globalBans.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot.globalBans.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator,
                            timestamp = x.Value.Timestamp.Ticks
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update globalbans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("globalbans", DatabaseColumnLists.globalbans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.globalbans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                            cmd.Parameters.AddWithValue($"timestamp{i}", DatabaseInserts[i].timestamp);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.globalbans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the submission_guild_bans table", ex);
                    }

                if (_bot.submittedUrls.Count > 0)
                    try
                    {
                        List<DatabaseSubmittedUrls> DatabaseInserts = _bot.submittedUrls.Select(x => new DatabaseSubmittedUrls
                        {
                            messageid = x.Key,
                            url = x.Value.Url,
                            submitter = x.Value.Submitter,
                            guild = x.Value.GuildOrigin
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occurred while trying to update active_url_submissions in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("active_url_submissions", DatabaseColumnLists.active_url_submissions);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.active_url_submissions, i);

                            cmd.Parameters.AddWithValue($"messageid{i}", DatabaseInserts[i].messageid);
                            cmd.Parameters.AddWithValue($"url{i}", DatabaseInserts[i].url);
                            cmd.Parameters.AddWithValue($"submitter{i}", DatabaseInserts[i].submitter);
                            cmd.Parameters.AddWithValue($"guild{i}", DatabaseInserts[i].guild);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.active_url_submissions);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An exception occurred while trying to update the active_url_submissions table", ex);
                    }

                await Task.Delay(1000);
                GC.Collect();
            });

            queuedUpdates.Add(key, Important);

            if (Important)
            {
                while (!key.IsCompleted && queuedUpdates.ContainsKey(key))
                {
                    await Task.Delay(100);
                }
                return;
            }
        }
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
