namespace Project_Ichigo;
internal class DatabaseHelper
{
    private List<Task> queuedUpdates = new();

    public async Task QueueWatcher()
    {
        while (true)
        {
            if (queuedUpdates.Any(x => x.IsCompleted))
                foreach (var task in queuedUpdates.Where(x => x.IsCompleted).ToList())
                    queuedUpdates.Remove(task);

            foreach (var task in queuedUpdates.Where(x => x.Status == TaskStatus.Created).ToList())
            {
                task.Start();
                await Task.Delay(30000);
            }

            await Task.Delay(1000);
        }
    }

    public async Task SyncDatabase()
    {
        if (queuedUpdates.Count < 2)
            queuedUpdates.Add(new Task(async () =>
            {
                try
                {
                    List<DatabaseServerSettings> DatabaseInserts = Bot._guilds.Servers.Select(x => new DatabaseServerSettings
                    {
                        serverid = x.Key,
                        phishing_detect = x.Value.PhishingDetectionSettings.DetectPhishing,
                        phishing_type = Convert.ToInt32(x.Value.PhishingDetectionSettings.PunishmentType),
                        phishing_reason = x.Value.PhishingDetectionSettings.CustomPunishmentReason,
                        phishing_time = Convert.ToInt64(x.Value.PhishingDetectionSettings.CustomPunishmentLength.TotalSeconds)
                    }).ToList();

                    if (Bot.databaseConnection == null)
                    {
                        throw new Exception($"Exception occured while trying to update guilds in database: Database connection not present");
                    }

                    var cmd = Bot.databaseConnection.CreateCommand();
                    cmd.CommandText = @$"INSERT INTO guilds ( serverid, phishing_detect, phishing_type, phishing_reason, phishing_time ) VALUES ";

                    for (int i = 0; i < DatabaseInserts.Count; i++)
                    {
                        cmd.CommandText += @$"( @serverid{i}, @phishing_detect{i}, @phishing_type{i}, @phishing_reason{i}, @phishing_time{i} ), ";

                        cmd.Parameters.AddWithValue($"serverid{i}", DatabaseInserts[i].serverid);
                        cmd.Parameters.AddWithValue($"phishing_detect{i}", DatabaseInserts[i].phishing_detect);
                        cmd.Parameters.AddWithValue($"phishing_type{i}", DatabaseInserts[i].phishing_type);
                        cmd.Parameters.AddWithValue($"phishing_reason{i}", DatabaseInserts[i].phishing_reason);
                        cmd.Parameters.AddWithValue($"phishing_time{i}", DatabaseInserts[i].phishing_time);
                    }

                    cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                    cmd.CommandText += " ON DUPLICATE KEY UPDATE " +
                                       "phishing_detect=values(phishing_detect), " +
                                       "phishing_type=values(phishing_type), " +
                                       "phishing_reason=values(phishing_reason), " +
                                       "phishing_time=values(phishing_time)";

                    cmd.Connection = Bot.databaseConnection;
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

                if (Bot._users.List.Count > 0)
                    try
                    {
                        List<DatabaseUsers> DatabaseInserts = Bot._users.List.Select(x => new DatabaseUsers
                        {
                            userid = x.Key,
                            submission_accepted_tos = x.Value.UrlSubmissions.AcceptedTOS,
                            submission_accepted_submissions = JsonConvert.SerializeObject(x.Value.UrlSubmissions.AcceptedSubmissions),
                            submission_last_datetime = x.Value.UrlSubmissions.LastTime
                        }).ToList();

                        if (Bot.databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update users in database: Database connection not present");
                        }

                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = @$"INSERT INTO users ( userid, submission_accepted_tos, submission_accepted_submissions, submission_last_datetime ) VALUES ";

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += @$"( @userid{i}, @submission_accepted_tos{i}, @submission_accepted_submissions{i}, @submission_last_datetime{i} ), ";

                            cmd.Parameters.AddWithValue($"userid{i}", DatabaseInserts[i].userid);
                            cmd.Parameters.AddWithValue($"submission_accepted_tos{i}", DatabaseInserts[i].submission_accepted_tos);
                            cmd.Parameters.AddWithValue($"submission_accepted_submissions{i}", DatabaseInserts[i].submission_accepted_submissions);
                            cmd.Parameters.AddWithValue($"submission_last_datetime{i}", DatabaseInserts[i].submission_last_datetime);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += " ON DUPLICATE KEY UPDATE " +
                                           "submission_accepted_tos=values(submission_accepted_tos), " +
                                           "submission_accepted_submissions=values(submission_accepted_submissions), " +
                                           "submission_last_datetime=values(submission_last_datetime)";

                        cmd.Connection = Bot.databaseConnection;
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

                if (Bot._submissionBans.BannedUsers.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = Bot._submissionBans.BannedUsers.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (Bot.databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update user_submission_bans in database: Database connection not present");
                        }

                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = @$"INSERT INTO user_submission_bans ( id, reason, moderator ) VALUES ";

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += @$"( @id{i}, @reason{i}, @moderator{i} ), ";

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += " ON DUPLICATE KEY UPDATE " +
                                           "reason=values(reason), " +
                                           "moderator=values(moderator)";

                        cmd.Connection = Bot.databaseConnection;
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

                if (Bot._submissionBans.BannedGuilds.Count > 0)
                    try
                    {
                        List<DatabaseBanInfo> DatabaseInserts = Bot._submissionBans.BannedGuilds.Select(x => new DatabaseBanInfo
                        {
                            id = x.Key,
                            reason = x.Value.Reason,
                            moderator = x.Value.Moderator
                        }).ToList();

                        if (Bot.databaseConnection == null)
                        {
                            throw new Exception($"Exception occured while trying to update guild_submission_bans in database: Database connection not present");
                        }

                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = @$"INSERT INTO guild_submission_bans ( id, reason, moderator ) VALUES ";

                        for (int i = 0; i < DatabaseInserts.Count; i++)
                        {
                            cmd.CommandText += @$"( @id{i}, @reason{i}, @moderator{i} ), ";

                            cmd.Parameters.AddWithValue($"id{i}", DatabaseInserts[i].id);
                            cmd.Parameters.AddWithValue($"reason{i}", DatabaseInserts[i].reason);
                            cmd.Parameters.AddWithValue($"moderator{i}", DatabaseInserts[i].moderator);
                        }

                        cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.LastIndexOf(','), 2);
                        cmd.CommandText += " ON DUPLICATE KEY UPDATE " +
                                           "reason=values(reason), " +
                                           "moderator=values(moderator)";

                        cmd.Connection = Bot.databaseConnection;
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

                await Task.Delay(1000);
                GC.Collect();
            }));
    }
}
