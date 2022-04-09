namespace Project_Ichigo.Database;
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
                await Task.Delay(1800000);

                if (Disposed)
                    return;

                _ = SyncDatabase();
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
                    await Task.Delay(60000, tokenSource.Token);
                }

                await Task.Delay(1000);
            }
            catch (TaskCanceledException)
            {
                continue;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public static async Task<DatabaseClient> InitializeDatabase(Bot _bot)
    {
        var databaseClient = new DatabaseClient
        {
            _bot = _bot,

            mainDatabaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};Connection Timeout=60;Database={Secrets.Secrets.MainDatabaseName};"),
            guildDatabaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};Connection Timeout=60;Database={Secrets.Secrets.GuildDatabaseName};")
        };
        databaseClient._helper = new(databaseClient);
        databaseClient._queue = new();

        databaseClient.mainDatabaseConnection.Open();
        databaseClient.guildDatabaseConnection.Open();

        try
        {
            var MainTables = await databaseClient._helper.ListTables(databaseClient.mainDatabaseConnection);

            foreach (var b in DatabaseColumnLists.Tables)
            {
                if (!MainTables.Contains(b.Key))
                {
                    LogWarn($"Missing table '{b.Key}'. Creating..");
                    string sql = $"CREATE TABLE `{Secrets.Secrets.MainDatabaseName}`.`{b.Key}` ( {string.Join(", ", b.Value.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(b.Value.Any(x => x.Primary) ? $", PRIMARY KEY (`{b.Value.First(x => x.Primary).Name}`)" : "")})";

                    var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = databaseClient.mainDatabaseConnection;

                    await databaseClient._queue.RunCommand(cmd);
                    LogInfo($"Created table '{b.Key}'.");
                }

                var Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);

                foreach (var col in b.Value)
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        LogWarn($"Missing column '{col.Name}' in '{b.Key}'. Creating..");
                        string sql = $"ALTER TABLE `{b.Key}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        LogInfo($"Created column '{col.Name}' in '{b.Key}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        LogWarn($"Wrong data type for column '{col.Name}' in '{b.Key}'");
                        string sql = $"ALTER TABLE `{b.Key}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = databaseClient.mainDatabaseConnection;

                        await databaseClient._queue.RunCommand(cmd);

                        LogInfo($"Changed column '{col.Name}' in '{b.Key}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await databaseClient._helper.ListColumns(databaseClient.mainDatabaseConnection, b.Key);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!b.Value.Any(x => x.Name == col.Key))
                    {
                        LogWarn($"Invalid column '{col.Key}' in '{b}'");

                        var cmd = databaseClient.mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE `{b}` DROP COLUMN `{col.Key}`";
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
        while (_queue.QueueCount() != 0)
            await Task.Delay(500);

        var GuildTables = await _helper.ListTables(guildDatabaseConnection);

        foreach (var b in _bot._guilds.Servers)
        {
            if (!GuildTables.Contains($"{b.Key}"))
            {
                LogWarn($"Missing table '{b.Key}'. Creating..");
                string sql = $"CREATE TABLE `{Secrets.Secrets.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", DatabaseColumnLists.guild_users.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.guild_users.Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.guild_users.First(x => x.Primary).Name}`)" : "")})";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);
                LogInfo($"Created table '{b.Key}'.");
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
                        LogWarn($"Missing column '{col.Name}' in '{b}'. Creating..");
                        string sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        LogInfo($"Created column '{col.Name}' in '{b}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        LogWarn($"Wrong data type for column '{col.Name}' in '{b}'");
                        string sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                        var cmd = guildDatabaseConnection.CreateCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = guildDatabaseConnection;

                        await _queue.RunCommand(cmd);

                        LogInfo($"Changed column '{col.Name}' in '{b}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await _helper.ListColumns(guildDatabaseConnection, b);
                    }
                }

                foreach (var col in Columns)
                {
                    if (!DatabaseColumnLists.guild_users.Any(x => x.Name == col.Key))
                    {
                        LogWarn($"Invalid column '{col.Key}' in '{b}'");

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
            LogWarn($"Missing table 'writetester'. Creating..");
            string sql = $"CREATE TABLE `{Secrets.Secrets.GuildDatabaseName}`.`writetester` ( {string.Join(", ", DatabaseColumnLists.Tables["writetester"].Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.Tables["writetester"].Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.Tables["writetester"].First(x => x.Primary).Name}`)" : "")})";

            var cmd = guildDatabaseConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = guildDatabaseConnection;

            await _queue.RunCommand(cmd);
            LogInfo($"Created table 'writetester'.");
        }

        var GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");

        foreach (var col in DatabaseColumnLists.Tables["writetester"])
        {
            if (!GuildColumns.ContainsKey(col.Name))
            {
                LogWarn($"Missing column '{col.Name}' in 'writetester'. Creating..");
                string sql = $"ALTER TABLE `writetester` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);

                LogInfo($"Created column '{col.Name}' in 'writetester'.");
                GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");
            }

            if (GuildColumns[col.Name].ToLower() != col.Type.ToLower())
            {
                LogWarn($"Wrong data type for column '{col.Name}' in 'writetester'");
                string sql = $"ALTER TABLE `writetester` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(!col.Nullable ? (col.Default.Length > 0 ? $" DEFAULT '{col.Default}'" : "") : "")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";

                var cmd = guildDatabaseConnection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = guildDatabaseConnection;

                await _queue.RunCommand(cmd);

                LogInfo($"Changed column '{col.Name}' in 'writetester' to datatype '{col.Type.ToUpper()}'.");
                GuildColumns = await _helper.ListColumns(guildDatabaseConnection, "writetester");
            }
        }

        foreach (var col in GuildColumns)
        {
            if (!DatabaseColumnLists.Tables["writetester"].Any(x => x.Name == col.Key))
            {
                LogWarn($"Invalid column '{col.Key}' in 'writetester'");

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
        })).CreateScheduleTask(DateTime.UtcNow.AddSeconds(20), "database-connection-watcher");

        if (Disposed)
            return;

        if (!connection.Ping())
        {
            try
            {
                LogWarn("Pinging the database failed, attempting reconnect.");
                connection.Open();
                LogInfo($"Reconnected to database.");
            }
            catch (Exception ex)
            {
                LogFatal($"Reconnecting to the database failed. Cannot sync changes to database", ex);
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
                LogWarn($"Creating a test value in database failed, reconnecting to database", ex);
                connection.Close();
                connection.Open();
                LogInfo($"Reconnected to database.");
            }
            catch (Exception ex1)
            {
                LogFatal($"Reconnecting to the database failed. Cannot sync changes to database", ex1);
                return;
            }
        }
    }

    public async Task SyncDatabase(bool Important = false)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (queuedUpdates.Count < 2 || Important)
        {
            Task key = new(async () =>
            {
                if (_bot._guilds.Servers.Count > 0)
                    try
                    {
                        List<DatabaseServerSettings> DatabaseInserts = _bot._guilds.Servers.Select(x => new DatabaseServerSettings
                        {
                            serverid = x.Key,

                            experience_use = x.Value.ExperienceSettings.UseExperience,
                            experience_boost_bumpreminder = x.Value.ExperienceSettings.BoostXpForBumpReminder,

                            auto_assign_role_id = x.Value.JoinSettings.AutoAssignRoleId,
                            joinlog_channel_id = x.Value.JoinSettings.JoinlogChannelId,
                            autoban_global_ban = x.Value.JoinSettings.AutoBanGlobalBans,

                            phishing_detect = x.Value.PhishingDetectionSettings.DetectPhishing,
                            phishing_type = Convert.ToInt32(x.Value.PhishingDetectionSettings.PunishmentType),
                            phishing_reason = x.Value.PhishingDetectionSettings.CustomPunishmentReason,
                            phishing_time = Convert.ToInt64(x.Value.PhishingDetectionSettings.CustomPunishmentLength.TotalSeconds),

                            bump_enabled = x.Value.BumpReminderSettings.Enabled,
                            bump_role = x.Value.BumpReminderSettings.RoleId,
                            bump_channel = x.Value.BumpReminderSettings.ChannelId,
                            bump_last_reminder = Convert.ToUInt64(x.Value.BumpReminderSettings.LastReminder.ToUniversalTime().Ticks),
                            bump_last_time = Convert.ToUInt64(x.Value.BumpReminderSettings.LastBump.ToUniversalTime().Ticks),
                            bump_last_user = x.Value.BumpReminderSettings.LastUserId,
                            bump_message = x.Value.BumpReminderSettings.MessageId,
                            bump_persistent_msg = x.Value.BumpReminderSettings.PersistentMessageId,
                            levelrewards = JsonConvert.SerializeObject(x.Value.LevelRewards),
                            auditlogcache = JsonConvert.SerializeObject(x.Value.ProcessedAuditLogs),
                            crosspostchannels = JsonConvert.SerializeObject(x.Value.CrosspostChannels),

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
                            actionlog_log_channels_modified = x.Value.ActionLogSettings.ChannelsModified
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update guilds in database: Database mainDatabaseConnection not present");
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

                            cmd.Parameters.AddWithValue($"levelrewards{i}", DatabaseInserts[i].levelrewards);
                            cmd.Parameters.AddWithValue($"auditlogcache{i}", DatabaseInserts[i].auditlogcache);
                            cmd.Parameters.AddWithValue($"crosspostchannels{i}", DatabaseInserts[i].crosspostchannels);

                            cmd.Parameters.AddWithValue($"bump_enabled{i}", DatabaseInserts[i].bump_enabled);
                            cmd.Parameters.AddWithValue($"bump_role{i}", DatabaseInserts[i].bump_role);
                            cmd.Parameters.AddWithValue($"bump_channel{i}", DatabaseInserts[i].bump_channel);
                            cmd.Parameters.AddWithValue($"bump_last_reminder{i}", DatabaseInserts[i].bump_last_reminder);
                            cmd.Parameters.AddWithValue($"bump_last_time{i}", DatabaseInserts[i].bump_last_time);
                            cmd.Parameters.AddWithValue($"bump_last_user{i}", DatabaseInserts[i].bump_last_user);
                            cmd.Parameters.AddWithValue($"bump_message{i}", DatabaseInserts[i].bump_message);
                            cmd.Parameters.AddWithValue($"bump_persistent_msg{i}", DatabaseInserts[i].bump_persistent_msg);

                            cmd.Parameters.AddWithValue($"phishing_detect{i}", DatabaseInserts[i].phishing_detect);
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
                            cmd.Parameters.AddWithValue($"actionlog_log_invites_modified{i}", DatabaseInserts[i].actionlog_log_invites_modified);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guilds);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'guilds'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guilds table", ex);
                    }

                var check = CheckGuildTables();
                check.Add(_bot._watcher);

                if (_bot._guilds.Servers.Count > 0)
                    foreach (var guild in _bot._guilds.Servers)
                        if (guild.Value.Members.Count > 0)
                        {
                            try
                            {
                                List<DatabaseMembers> DatabaseInserts = guild.Value.Members.Select(x => new DatabaseMembers
                                {
                                    userid = x.Key,

                                    experience = x.Value.Experience,
                                    experience_level = x.Value.Level,
                                    experience_last_message = Convert.ToUInt64(x.Value.Last_Message.ToUniversalTime().Ticks)
                                }).ToList();

                                if (mainDatabaseConnection == null)
                                {
                                    throw new Exception($"Exception occured while trying to update guilds in database: Database mainDatabaseConnection not present");
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
                                }

                                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                                cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guild_users);

                                cmd.Connection = guildDatabaseConnection;
                                await _queue.RunCommand(cmd);

                                LogDebug($"Inserted {DatabaseInserts.Count} rows into table '{guild.Key}'.");
                                DatabaseInserts.Clear();
                                DatabaseInserts = null;
                                cmd.Dispose();
                            }
                            catch (Exception ex)
                            {
                                LogError($"An exception occured while trying to update the {guild.Key} table", ex);
                            }
                        }

                if (_bot._users.List.Count > 0)
                    try
                    {
                        List<DatabaseUsers> DatabaseInserts = _bot._users.List.Select(x => new DatabaseUsers
                        {
                            userid = x.Key,
                            afk_since = Convert.ToUInt64(x.Value.AfkStatus.TimeStamp.ToUniversalTime().Ticks),
                            afk_reason = x.Value.AfkStatus.Reason,
                            afk_pings = JsonConvert.SerializeObject(x.Value.AfkStatus.Messages),
                            afk_pingamount = x.Value.AfkStatus.MessagesAmount,
                            experience_directmessageoptout = x.Value.ExperienceUserSettings.DirectMessageOptOut,
                            submission_accepted_tos = x.Value.UrlSubmissions.AcceptedTOS,
                            submission_accepted_submissions = JsonConvert.SerializeObject(x.Value.UrlSubmissions.AcceptedSubmissions),
                            submission_last_datetime = x.Value.UrlSubmissions.LastTime,
                            scoresaber_id = x.Value.ScoreSaber.Id
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update users in database: Database mainDatabaseConnection not present");
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
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.users);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'users'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the users table", ex);
                    }

                if (_bot._submissionBans.BannedUsers.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot._submissionBans.BannedUsers.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update user_submission_bans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("user_submission_bans", DatabaseColumnLists.user_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.user_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.user_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'user_submission_bans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the user_submission_bans table", ex);
                    }

                if (_bot._submissionBans.BannedGuilds.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot._submissionBans.BannedGuilds.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update guild_submission_bans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("guild_submission_bans", DatabaseColumnLists.guild_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.guild_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guild_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'guild_submission_bans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guild_submission_bans table", ex);
                    }

                if (_bot._globalBans.Users.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _bot._globalBans.Users.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update globalbans in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = _helper.GetSaveCommand("globalbans", DatabaseColumnLists.guild_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += _helper.GetValueCommand(DatabaseColumnLists.guild_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += _helper.GetOverwriteCommand(DatabaseColumnLists.guild_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await _queue.RunCommand(cmd);

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'globalbans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guild_submission_bans table", ex);
                    }

                if (_bot._submittedUrls.Urls.Count > 0)
                    try
                    {
                        List<DatabaseSubmittedUrls> DatabaseInserts = _bot._submittedUrls.Urls.Select(x => new DatabaseSubmittedUrls
                        {
                            messageid = x.Key,
                            url = x.Value.Url,
                            submitter = x.Value.Submitter,
                            guild = x.Value.GuildOrigin
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update active_url_submissions in database: Database mainDatabaseConnection not present");
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

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'active_url_submissions'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the active_url_submissions table", ex);
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

    public async Task Dispose()
    {
        foreach (var b in GetScheduleTasks())
            if (b.Value.customId == "database-connection-watcher")
                DeleteScheduleTask(b.Key);

        int timeout = 0;

        while (timeout < 30 && _queue.QueueCount() != 0)
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
