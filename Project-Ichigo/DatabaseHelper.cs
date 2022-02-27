namespace Project_Ichigo;
internal class DatabaseHelper
{
    internal MySqlConnection databaseConnection { get; set; }
    internal ServerInfo _guilds { private set; get; }
    internal Users _users { private get; set; }
    internal SubmissionBans _submissionBans { private get; set; }
    internal SubmittedUrls _submittedUrls { private get; set; }

    private Dictionary<Task, bool> queuedUpdates = new();

    public async Task QueueWatcher()
    {
        CancellationTokenSource tokenSource = new();

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(300000);
                _ = SyncDatabase();
            }
        });

        _ = Task.Run(async () =>
        {
            while (true)
            {
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
            try
            {
                if (queuedUpdates.Any(x => x.Key.IsCompleted))
                    foreach (var task in queuedUpdates.Where(x => x.Key.IsCompleted).ToList())
                        queuedUpdates.Remove(task.Key);

                foreach (var task in queuedUpdates.Where(x => x.Key.Status == TaskStatus.Created).ToList())
                {
                    task.Key.Start();
                    await Task.Delay(30000, tokenSource.Token);
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

    public static async Task<DatabaseHelper> InitializeDatabase(ServerInfo guilds, Users users, SubmissionBans submissionBans, SubmittedUrls submittedUrls)
    {
        var helper = new DatabaseHelper
        {
            _guilds = guilds,
            _users = users,
            _submissionBans = submissionBans,
            _submittedUrls = submittedUrls,

            databaseConnection = new MySqlConnection($"Server={Secrets.Secrets.DatabaseUrl};Port={Secrets.Secrets.DatabasePort};User Id={Secrets.Secrets.DatabaseUserName};Password={Secrets.Secrets.DatabasePassword};")
        };
        helper.databaseConnection.Open();

        await helper.SelectDatabase(Secrets.Secrets.DatabaseName, true);

        var Tables = await helper.ListTables();

        foreach (var b in DatabaseColumnLists.Tables)
        {
            if (!Tables.Contains(b.Key))
            {
                LogWarn($"Missing table '{b.Key}'. Creating..");
                string sql = $"CREATE TABLE `{Secrets.Secrets.DatabaseName}`.`{b.Key}` ( {string.Join(", ", b.Value.Select(x => $"`{x.Name}` {x.Type.ToUpper()}{(x.Collation != "" ? $" CHARACTER SET {x.Collation.Remove(x.Collation.IndexOf("_"), x.Collation.Length - x.Collation.IndexOf("_"))} COLLATE {x.Collation}" : "")}{(x.Nullable ? " NULL" : " NOT NULL")}"))}{(b.Value.Any(x => x.Primary) ? $", PRIMARY KEY (`{b.Value.First(x => x.Primary).Name}`)" : "")})";
                
                await helper.databaseConnection.ExecuteAsync(sql);
                LogInfo($"Created table '{b.Key}'.");
            }

            var Columns = await helper.ListColumns(b.Key);

            foreach (var col in b.Value)
            {
                if (!Columns.ContainsKey(col.Name))
                {
                    LogWarn($"Missing column '{col.Name}' in '{b.Key}'. Creating..");
                    string sql = $"ALTER TABLE `{b.Key}` ADD `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                    await helper.databaseConnection.ExecuteAsync(sql);
                    LogInfo($"Created column '{col.Name}' in '{b.Key}'.");
                    Columns = await helper.ListColumns(b.Key);
                }

                if (Columns[col.Name].ToLower() != col.Type.ToLower())
                {
                    LogWarn($"Wrong data type for column '{col.Name}' in '{b.Key}'");
                    string sql = $"ALTER TABLE `{b.Key}` CHANGE `{col.Name}` `{col.Name}` {col.Type.ToUpper()}{(col.Collation != "" ? $" CHARACTER SET {col.Collation.Remove(col.Collation.IndexOf("_"), col.Collation.Length - col.Collation.IndexOf("_"))} COLLATE {col.Collation}" : "")}{(col.Nullable ? " NULL" : " NOT NULL")}{(col.Primary ? $", ADD PRIMARY KEY (`{col.Name}`)" : "")}";
                    await helper.databaseConnection.ExecuteAsync(sql);
                    LogInfo($"Changed column '{col.Name}' in '{b.Key}' to datatype '{col.Type.ToUpper()}'.");
                    Columns = await helper.ListColumns(b.Key);
                }
            }
        }

        return helper;
    }

    public async Task SelectDatabase(string databaseName, bool CreateIfNotExist = false)
    {
        if (CreateIfNotExist)
            await databaseConnection.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS {databaseName}");

        await databaseConnection.ExecuteAsync($"USE {databaseName}");
    }

    public async Task<IEnumerable<string>> ListTables()
    {
        List<string> SavedTables = new();

        using (IDataReader reader = databaseConnection.ExecuteReader($"SHOW TABLES"))
        {
            while (reader.Read())
            {
                SavedTables.Add(reader.GetString(0));
            }
        }

        return SavedTables as IEnumerable<string>;
    }
    
    public async Task<Dictionary<string, string>> ListColumns(string table)
    {
        Dictionary<string, string> Columns = new();

        using (IDataReader reader = databaseConnection.ExecuteReader($"SHOW FIELDS FROM {table}"))
        {
            while (reader.Read())
            {
                Columns.Add(reader.GetString(0), reader.GetString(1));
            }
        }

        return Columns;
    }

    public async Task DeleteRow(string table, string row_match, string value)
    {
        var cmd = databaseConnection.CreateCommand();
        cmd.CommandText = $"DELETE FROM {table} WHERE {row_match}='{value}'";
        cmd.Connection = databaseConnection;
        await cmd.ExecuteNonQueryAsync();
    }

    public string GetLoadCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        return $"SELECT {string.Join(", ", columns.Select(x => x.Name))} FROM {table}";
    }
    
    public string GetSaveCommand(string table, List<DatabaseColumnLists.Column> columns)
    {
        return $"INSERT INTO {table} ( {string.Join(", ", columns.Select(x => x.Name))} ) VALUES ";
    }
    
    public string GetValueCommand(List<DatabaseColumnLists.Column> columns, int i)
    {
        return $"( {string.Join(", ", columns.Select(x => $"@{x.Name}{i}"))} ), ";
    }
    
    public string GetOverwriteCommand(List<DatabaseColumnLists.Column> columns)
    {
        return $" ON DUPLICATE KEY UPDATE {string.Join(", ", columns.Select(x => $"{x.Name}=values({x.Name})"))}";
    }

    public async Task SyncDatabase(bool Important = false)
    {
        if (queuedUpdates.Count < 2 || Important)
        {
            Task key = new(async () =>
            {
                try
                {
                    List<DatabaseServerSettings> DatabaseInserts = _guilds.Servers.Select(x => new DatabaseServerSettings
                    {
                        serverid = x.Key,
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

                    if (databaseConnection == null)
                    {
                        throw new Exception($"Exception occured while trying to update guilds in database: Database connection not present");
                    }

                    var cmd = databaseConnection.CreateCommand();
                    cmd.CommandText = GetSaveCommand("guilds", DatabaseColumnLists.guilds);

                    for (int i = 0; i < DatabaseInserts.Count; i++)
                    {
                        cmd.CommandText += GetValueCommand(DatabaseColumnLists.guilds, i);

                        cmd.Parameters.AddWithValue($"serverid{i}", DatabaseInserts[i].serverid);

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

                    cmd.Connection = databaseConnection;
                    await cmd.ExecuteNonQueryAsync();

                    LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'guilds'.");
                    DatabaseInserts.Clear();
                    DatabaseInserts = null;
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    LogError($"An exception occured while trying to update the guilds table: {ex}");
                }

                if (_users.List.Count > 0)
                    try
                    {
                        List<DatabaseUsers> DatabaseInserts = _users.List.Select(x => new DatabaseUsers
                        {
                            userid = x.Key,
                            afk_since = Convert.ToUInt64(x.Value.AfkStatus.TimeStamp.ToUniversalTime().Ticks),
                            afk_reason = x.Value.AfkStatus.Reason,
                            submission_accepted_tos = x.Value.UrlSubmissions.AcceptedTOS,
                            submission_accepted_submissions = JsonConvert.SerializeObject(x.Value.UrlSubmissions.AcceptedSubmissions),
                            submission_last_datetime = x.Value.UrlSubmissions.LastTime,
                            scoresaber_id = x.Value.ScoreSaber.Id
                        }).ToList();

                        if (databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update users in database: Database connection not present");
                        }

                        var cmd = databaseConnection.CreateCommand();
                        cmd.CommandText = cmd.CommandText = GetSaveCommand("users", DatabaseColumnLists.users);

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += GetValueCommand(DatabaseColumnLists.users, i);

                            cmd.Parameters.AddWithValue($"userid{i}", DatabaseInserts[i].userid);
                            cmd.Parameters.AddWithValue($"scoresaber_id{i}", DatabaseInserts[i].scoresaber_id);
                            cmd.Parameters.AddWithValue($"afk_since{i}", DatabaseInserts[i].afk_since);
                            cmd.Parameters.AddWithValue($"afk_reason{i}", DatabaseInserts[i].afk_reason);
                            cmd.Parameters.AddWithValue($"submission_accepted_tos{i}", DatabaseInserts[i].submission_accepted_tos);
                            cmd.Parameters.AddWithValue($"submission_accepted_submissions{i}", DatabaseInserts[i].submission_accepted_submissions);
                            cmd.Parameters.AddWithValue($"submission_last_datetime{i}", DatabaseInserts[i].submission_last_datetime);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += GetOverwriteCommand(DatabaseColumnLists.users);

                        cmd.Connection = databaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'users'.");
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

                        if (databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update user_submission_bans in database: Database connection not present");
                        }

                        var cmd = databaseConnection.CreateCommand();
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

                        cmd.Connection = databaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'user_submission_bans'.");
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

                        if (databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update guild_submission_bans in database: Database connection not present");
                        }

                        var cmd = databaseConnection.CreateCommand();
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

                        cmd.Connection = databaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'guild_submission_bans'.");
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

                        if (databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update active_url_submissions in database: Database connection not present");
                        }

                        var cmd = databaseConnection.CreateCommand();
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

                        cmd.Connection = databaseConnection;
                        await cmd.ExecuteNonQueryAsync();

                        LogInfo($"Inserted {DatabaseInserts.Count} rows into table 'active_url_submissions'.");
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
                while (!key.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                await Task.Delay(2000);
            }
        }
    }
}
