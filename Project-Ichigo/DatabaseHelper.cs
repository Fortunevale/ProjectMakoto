namespace Project_Ichigo;
internal class DatabaseHelper
{
    internal MySqlConnection mainDatabaseConnection { get; set; }
    internal MySqlConnection guildDatabaseConnection { get; set; }
    internal ServerInfo _guilds { private set; get; }
    internal Users _users { private get; set; }
    internal SubmissionBans _submissionBans { private get; set; }
    internal GlobalBans _globalbans { private get; set; }
    internal SubmittedUrls _submittedUrls { private get; set; }
    public TaskWatcher.TaskWatcher _watcher { private get; set; }

    internal bool Disposed { get; private set; } = false;

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

    public static async Task<DatabaseHelper> InitializeDatabase(TaskWatcher.TaskWatcher watcher, ServerInfo guilds, Users users, SubmissionBans submissionBans, SubmittedUrls submittedUrls, GlobalBans globalbans)
    {
        var helper = new DatabaseHelper
        {
            _guilds = guilds,
            _users = users,
            _submissionBans = submissionBans,
            _submittedUrls = submittedUrls,
            _globalbans = globalbans,
            _watcher = watcher,

            mainDatabaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};Connection Timeout=60;"),
            guildDatabaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};Connection Timeout=60;")
        };
        helper.mainDatabaseConnection.Open();
        helper.guildDatabaseConnection.Open();

        await helper.SelectDatabase(helper.mainDatabaseConnection, Secrets.Secrets.MainDatabaseName, true);
        await helper.SelectDatabase(helper.guildDatabaseConnection, Secrets.Secrets.GuildDatabaseName, true);

        try
        {
            var MainTables = await helper.ListTables(helper.mainDatabaseConnection);

            foreach (var b in DatabaseColumnLists.Tables)
            {
                if (!MainTables.Contains(b.Key))
                {
                    LogWarn($"Missing table '{b.Key}'. Creating..");
                    string sql = $"CREATE TABLE `{Secrets.Secrets.MainDatabaseName}`.`{b.Key}` ( {string.Join(", ", b.Value.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(b.Value.Any(x => x.Primary) ? $", PRIMARY KEY (`{b.Value.First(x => x.Primary).Name}`)" : "")})";

                    await helper.mainDatabaseConnection.ExecuteAsync(sql);
                    LogInfo($"Created table '{b.Key}'.");
                }

                var Columns = await helper.ListColumns(helper.mainDatabaseConnection, b.Key);

                foreach (var col in b.Value)
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        LogWarn($"Missing column '{col.Name}' in '{b.Key}'. Creating..");
                        string sql = $"ALTER TABLE `{b.Key}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                        await helper.mainDatabaseConnection.ExecuteAsync(sql);
                        LogInfo($"Created column '{col.Name}' in '{b.Key}'.");
                        Columns = await helper.ListColumns(helper.mainDatabaseConnection, b.Key);
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        LogWarn($"Wrong data type for column '{col.Name}' in '{b.Key}'");
                        string sql = $"ALTER TABLE `{b.Key}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                        await helper.mainDatabaseConnection.ExecuteAsync(sql);
                        LogInfo($"Changed column '{col.Name}' in '{b.Key}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await helper.ListColumns(helper.mainDatabaseConnection, b.Key);
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        new Task(new Action(async () =>
        {
            _ = helper.CheckDatabaseConnection(helper.mainDatabaseConnection);
            await Task.Delay(10000);
            _ = helper.CheckDatabaseConnection(helper.guildDatabaseConnection);
        })).CreateScheduleTask(DateTime.UtcNow.AddSeconds(10), "database-connection-watcher");

        return helper;
    }

    public async Task CheckGuildTables()
    {
        var GuildTables = await ListTables(guildDatabaseConnection);

        foreach (var b in _guilds.Servers)
        {
            if (!GuildTables.Contains($"{b.Key}"))
            {
                LogWarn($"Missing table '{b.Key}'. Creating..");
                string sql = $"CREATE TABLE `{Secrets.Secrets.GuildDatabaseName}`.`{b.Key}` ( {string.Join(", ", DatabaseColumnLists.guild_users.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.guild_users.Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.guild_users.First(x => x.Primary).Name}`)" : "")})";

                await guildDatabaseConnection.ExecuteAsync(sql);
                LogInfo($"Created table '{b.Key}'.");
            }
        }

        GuildTables = await ListTables(guildDatabaseConnection);

        foreach (var b in GuildTables)
        {
            if (b != "writetester")
            {
                var Columns = await ListColumns(guildDatabaseConnection, b);

                foreach (var col in DatabaseColumnLists.guild_users)
                {
                    if (!Columns.ContainsKey(col.Name))
                    {
                        LogWarn($"Missing column '{col.Name}' in '{b}'. Creating..");
                        string sql = $"ALTER TABLE `{b}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                        await guildDatabaseConnection.ExecuteAsync(sql);
                        LogInfo($"Created column '{col.Name}' in '{b}'.");
                        Columns = await ListColumns(guildDatabaseConnection, "writetester");
                    }

                    if (Columns[col.Name].ToLower() != col.Type.ToLower())
                    {
                        LogWarn($"Wrong data type for column '{col.Name}' in '{b}'");
                        string sql = $"ALTER TABLE `{b}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                        await guildDatabaseConnection.ExecuteAsync(sql);
                        LogInfo($"Changed column '{col.Name}' in '{b}' to datatype '{col.Type.ToUpper()}'.");
                        Columns = await ListColumns(guildDatabaseConnection, "writetester");
                    }
                }
            }
        }

        if (!GuildTables.Contains("writetester"))
        {
            LogWarn($"Missing table 'writetester'. Creating..");
            string sql = $"CREATE TABLE `{Secrets.Secrets.GuildDatabaseName}`.`writetester` ( {string.Join(", ", DatabaseColumnLists.Tables["writetester"].Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(DatabaseColumnLists.Tables["writetester"].Any(x => x.Primary) ? $", PRIMARY KEY (`{DatabaseColumnLists.Tables["writetester"].First(x => x.Primary).Name}`)" : "")})";

            await guildDatabaseConnection.ExecuteAsync(sql);
            LogInfo($"Created table 'writetester'.");
        }

        var GuildColumns = await ListColumns(guildDatabaseConnection, "writetester");

        foreach (var col in DatabaseColumnLists.Tables["writetester"])
        {
            if (!GuildColumns.ContainsKey(col.Name))
            {
                LogWarn($"Missing column '{col.Name}' in 'writetester'. Creating..");
                string sql = $"ALTER TABLE `writetester` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                await guildDatabaseConnection.ExecuteAsync(sql);
                LogInfo($"Created column '{col.Name}' in 'writetester'.");
                GuildColumns = await ListColumns(guildDatabaseConnection, "writetester");
            }

            if (GuildColumns[col.Name].ToLower() != col.Type.ToLower())
            {
                LogWarn($"Wrong data type for column '{col.Name}' in 'writetester'");
                string sql = $"ALTER TABLE `writetester` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                await guildDatabaseConnection.ExecuteAsync(sql);
                LogInfo($"Changed column '{col.Name}' in 'writetester' to datatype '{col.Type.ToUpper()}'.");
                GuildColumns = await ListColumns(guildDatabaseConnection, "writetester");
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
                await SelectDatabase(connection, Secrets.Secrets.MainDatabaseName, true);
                LogInfo($"Reconnected to database.");
            }
            catch (Exception ex)
            {
                LogFatal($"Reconnecting to the database failed. Cannot sync changes to database: {ex}");
                return;
            }
        }

        try
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = GetSaveCommand("writetester", DatabaseColumnLists.writetester);

            cmd.CommandText += GetValueCommand(DatabaseColumnLists.writetester, 1);

            cmd.Parameters.AddWithValue($"aaa1", 1);

            cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
            cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.writetester);

            cmd.Connection = connection;
            await cmd.ExecuteNonQueryAsync();

            await DeleteRow(connection, "writetester", "aaa", "1");
        }
        catch (Exception ex)
        {
            try
            {
                LogWarn($"Creating a test value in database failed, reconnecting to database: {ex}");
                connection.Close();
                connection.Open();
                await SelectDatabase(connection, Secrets.Secrets.MainDatabaseName, true);
                LogInfo($"Reconnected to database.");
            }
            catch (Exception ex1)
            {
                LogFatal($"Reconnecting to the database failed. Cannot sync changes to database: {ex1}");
                return;
            }
        }
    }

    public async Task SelectDatabase(MySqlConnection connection, string databaseName, bool CreateIfNotExist = false)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (CreateIfNotExist)
            await connection.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS {databaseName}");

        await connection.ChangeDatabaseAsync(databaseName);
    }

    public async Task<IEnumerable<string>> ListTables(MySqlConnection connection)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        List<string> SavedTables = new();

        using (IDataReader reader = connection.ExecuteReader($"SHOW TABLES"))
        {
            while (reader.Read())
            {
                SavedTables.Add(reader.GetString(0));
            }
        }

        return SavedTables as IEnumerable<string>;
    }

    public async Task<Dictionary<string, string>> ListColumns(MySqlConnection connection, string table)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        Dictionary<string, string> Columns = new();

        using (IDataReader reader = connection.ExecuteReader($"SHOW FIELDS FROM `{table}`"))
        {
            while (reader.Read())
            {
                Columns.Add(reader.GetString(0), reader.GetString(1));
            }
        }

        return Columns;
    }

    public async Task DeleteRow(MySqlConnection connection, string table, string row_match, string value)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM `{table}` WHERE {row_match}='{value}'";
        cmd.Connection = connection;
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DropTable(MySqlConnection connection, string table)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP TABLE IF EXISTS `{table}`";
        cmd.Connection = connection;
        await cmd.ExecuteNonQueryAsync();
    }

    public string GetLoadCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        return $"SELECT {string.Join(", ", columns.Select(x => x.Name))} FROM `{table}`";
    }

    public string GetSaveCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        return $"INSERT INTO `{table}` ( {string.Join(", ", columns.Select(x => x.Name))} ) VALUES ";
    }

    public string GetValueCommand(List<DatabaseColumnLists.Column> columns, int i)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        return $"( {string.Join(", ", columns.Select(x => $"@{x.Name}{i}"))} ), ";
    }

    public string GetOverwriteCommand(List<DatabaseColumnLists.Column> columns)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        return $" ON DUPLICATE KEY UPDATE {string.Join(", ", columns.Select(x => $"{x.Name}=values({x.Name})"))}";
    }

    public async Task SyncDatabase(bool Important = false)
    {
        if (Disposed)
            throw new Exception("DatabaseHelper is disposed");

        if (queuedUpdates.Count < 2 || Important)
        {
            Task key = new(async () =>
            {
                try
                {
                    if (!mainDatabaseConnection.Ping())
                    {
                        try
                        {
                            LogWarn("Pinging the database failed, attempting reconnect.");
                            mainDatabaseConnection.Open();
                            await SelectDatabase(mainDatabaseConnection, Secrets.Secrets.MainDatabaseName, true);
                        }
                        catch (Exception ex)
                        {
                            LogFatal($"Reconnecting to the database failed. Cannot sync changes to database: {ex}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        LogWarn($"Pinging the database failed, attempting reconnect: {ex}");
                        mainDatabaseConnection.Close();
                        mainDatabaseConnection.Open();
                        await SelectDatabase(mainDatabaseConnection, Secrets.Secrets.MainDatabaseName, true);
                        LogInfo($"Reconnected to database.");
                    }
                    catch (Exception ex1)
                    {
                        LogFatal($"Reconnecting to the database failed. Cannot sync changes to database: {ex1}");
                        return;
                    }
                }

                if (_guilds.Servers.Count > 0)
                    try
                    {
                        List<DatabaseServerSettings> DatabaseInserts = _guilds.Servers.Select(x => new DatabaseServerSettings
                        {
                            serverid = x.Key,

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
                            bump_persistent_msg = x.Value.BumpReminderSettings.PersistentMessageId
                        }).ToList();

                        if (mainDatabaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update guilds in database: Database mainDatabaseConnection not present");
                        }

                        var cmd = mainDatabaseConnection.CreateCommand();
                        cmd.CommandText = GetSaveCommand("guilds", DatabaseColumnLists.guilds);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.guilds, i);

                            cmd.Parameters.AddWithValue($"serverid{i}", DatabaseInserts[i].serverid);

                            cmd.Parameters.AddWithValue($"auto_assign_role_id{i}", DatabaseInserts[i].auto_assign_role_id);
                            cmd.Parameters.AddWithValue($"joinlog_channel_id{i}", DatabaseInserts[i].joinlog_channel_id);
                            cmd.Parameters.AddWithValue($"autoban_global_ban{i}", DatabaseInserts[i].autoban_global_ban);

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
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.guilds);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'guilds'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guilds table: {ex}");
                    }

                var check = CheckGuildTables();

                check.Add(_watcher);
                check.Wait();

                if (_guilds.Servers.Count > 0)
                    foreach (var guild in _guilds.Servers)
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
                                cmd.CommandText = GetSaveCommand($"{guild.Key}", DatabaseColumnLists.guild_users);

                                for (int i = 0; i < DatabaseInserts.Count; i++)
                                {
                                    cmd.CommandText += GetValueCommand(DatabaseColumnLists.guild_users, i);

                                    cmd.Parameters.AddWithValue($"userid{i}", DatabaseInserts[i].userid);

                                    cmd.Parameters.AddWithValue($"experience{i}", DatabaseInserts[i].experience);
                                    cmd.Parameters.AddWithValue($"level{i}", DatabaseInserts[i].experience_level);
                                    cmd.Parameters.AddWithValue($"experience_last_message{i}", DatabaseInserts[i].experience_last_message);
                                }

                                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                                cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.guild_users);

                                cmd.Connection = guildDatabaseConnection;
                                await cmd.ExecuteNonQueryAsync();

                                LogDebug($"Inserted {DatabaseInserts.Count} rows into table '{guild.Key}'.");
                                DatabaseInserts.Clear();
                                DatabaseInserts = null;
                                cmd.Dispose();
                            }
                            catch (Exception ex)
                            {
                                LogError($"An exception occured while trying to update the {guild.Key} table: {ex}");
                            }
                        }

                if (_users.List.Count > 0)
                    try
                    {
                        List<DatabaseUsers> DatabaseInserts = _users.List.Select(x => new DatabaseUsers
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
                        cmd.CommandText = cmd.CommandText = GetSaveCommand("users", DatabaseColumnLists.users);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.users, i);

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
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.users);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'users'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the users table: {ex}");
                    }

                if (_submissionBans.BannedUsers.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _submissionBans.BannedUsers.Select(x => new DatabaseBanInfo
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
                        cmd.CommandText = GetSaveCommand("user_submission_bans", DatabaseColumnLists.user_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.user_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.user_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'user_submission_bans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the user_submission_bans table: {ex}");
                    }

                if (_submissionBans.BannedGuilds.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _submissionBans.BannedGuilds.Select(x => new DatabaseBanInfo
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
                        cmd.CommandText = GetSaveCommand("guild_submission_bans", DatabaseColumnLists.guild_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.guild_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.guild_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'guild_submission_bans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guild_submission_bans table: {ex}");
                    }

                if (_globalbans.Users.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = _globalbans.Users.Select(x => new DatabaseBanInfo
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
                        cmd.CommandText = GetSaveCommand("globalbans", DatabaseColumnLists.guild_submission_bans);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.guild_submission_bans, i);

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.guild_submission_bans);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'globalbans'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the guild_submission_bans table: {ex}");
                    }

                if (_submittedUrls.Urls.Count > 0)
                    try
                    {
                        List<DatabaseSubmittedUrls> DatabaseInserts = _submittedUrls.Urls.Select(x => new DatabaseSubmittedUrls
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
                        cmd.CommandText = GetSaveCommand("active_url_submissions", DatabaseColumnLists.active_url_submissions);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.active_url_submissions, i);

                            cmd.Parameters.AddWithValue($"messageid{i}", DatabaseInserts[i].messageid);
                            cmd.Parameters.AddWithValue($"url{i}", DatabaseInserts[i].url);
                            cmd.Parameters.AddWithValue($"submitter{i}", DatabaseInserts[i].submitter);
                            cmd.Parameters.AddWithValue($"guild{i}", DatabaseInserts[i].guild);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.active_url_submissions);

                        cmd.Connection = mainDatabaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogDebug($"Inserted {DatabaseInserts.Count} rows into table 'active_url_submissions'.");
                        DatabaseInserts.Clear();
                        DatabaseInserts = null;
                        cmd.Dispose();
                    }
                    catch (Exception ex)
                    {
                        LogError($"An exception occured while trying to update the active_url_submissions table: {ex}");
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

        Disposed = true;

        await mainDatabaseConnection.CloseAsync();
    }
}
