namespace Project_Ichigo.PhishingProtection;

internal class SubmissionEvents
{
    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (Bot._submittedUrls.Urls.ContainsKey(e.Message.Id))
            {
                if (!e.User.IsMaintenance())
                    return;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Interaction.Data.CustomId == "accept_submission")
                {
                    Bot._phishingUrls.List.Add(Bot._submittedUrls.Urls[e.Message.Id].Url, new PhishingUrls.UrlInfo
                    {
                        Origin = new(),
                        Submitter = Bot._submittedUrls.Urls[e.Message.Id].Submitter,
                        Url = Bot._submittedUrls.Urls[e.Message.Id].Url
                    });

                    Bot._submittedUrls.Urls.Remove(e.Message.Id);

                    try
                    {
                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = $"DELETE FROM active_url_submissions WHERE messageid='{e.Message.Id}'";
                        cmd.Connection = Bot.databaseConnection;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();

                    try
                    {
                        _ = new PhishingUrlUpdater().UpdateDatabase(Bot._phishingUrls, new());
                    }
                    catch (Exception ex)
                    {
                        LogError($"{ex}");
                    }
                }
                else if (e.Interaction.Data.CustomId == "deny_submission")
                {
                    Bot._submittedUrls.Urls.Remove(e.Message.Id);

                    try
                    {
                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = $"DELETE FROM active_url_submissions WHERE messageid='{e.Message.Id}'";
                        cmd.Connection = Bot.databaseConnection;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_user")
                {
                    Bot._submissionBans.BannedUsers.Add(Bot._submittedUrls.Urls[e.Message.Id].Submitter, new SubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = $"DELETE FROM active_url_submissions WHERE messageid='{e.Message.Id}'";
                        cmd.Connection = Bot.databaseConnection;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }

                    Bot._submittedUrls.Urls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_guild")
                {
                    Bot._submissionBans.BannedGuilds.Add(Bot._submittedUrls.Urls[e.Message.Id].GuildOrigin, new SubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        var cmd = Bot.databaseConnection.CreateCommand();
                        cmd.CommandText = $"DELETE FROM active_url_submissions WHERE messageid='{e.Message.Id}'";
                        cmd.Connection = Bot.databaseConnection;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch { }

                    Bot._submittedUrls.Urls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }

            }
        });
    }
}
