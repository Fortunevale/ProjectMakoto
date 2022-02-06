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

    public async Task SyncGuilds()
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
                    cmd.CommandText += " ON DUPLICATE KEY UPDATE phishing_detect=values(phishing_detect), " +
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
                    GC.Collect();
                    throw;
                }
                finally
                {
                    await Task.Delay(1000);
                    GC.Collect();
                }
            }));
    }
}
