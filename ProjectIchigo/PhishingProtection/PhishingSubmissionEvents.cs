namespace ProjectIchigo;

internal class PhishingSubmissionEvents
{
    internal PhishingSubmissionEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (_bot._submittedUrls.List.ContainsKey(e.Message.Id))
            {
                if (!e.User.IsMaintenance(_bot._status))
                    return;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Interaction.Data.CustomId == "accept_submission")
                {
                    _bot._phishingUrls.List.Add(_bot._submittedUrls.List[e.Message.Id].Url, new PhishingUrls.UrlInfo
                    {
                        Origin = new(),
                        Submitter = _bot._submittedUrls.List[e.Message.Id].Submitter,
                        Url = _bot._submittedUrls.List[e.Message.Id].Url
                    });

                    _bot._submittedUrls.List.Remove(e.Message.Id);

                    try
                    {
                        await _bot._databaseClient._helper.DeleteRow(_bot._databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();

                    try
                    {
                        _ = new PhishingUrlUpdater(_bot).UpdateDatabase(_bot._phishingUrls, new());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to update database", ex);
                    }
                }
                else if (e.Interaction.Data.CustomId == "deny_submission")
                {
                    _bot._submittedUrls.List.Remove(e.Message.Id);

                    try
                    {
                        await _bot._databaseClient._helper.DeleteRow(_bot._databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_user")
                {
                    _bot._submissionBans.Users.Add(_bot._submittedUrls.List[e.Message.Id].Submitter, new PhishingSubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await _bot._databaseClient._helper.DeleteRow(_bot._databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _bot._submittedUrls.List.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_guild")
                {
                    _bot._submissionBans.Guilds.Add(_bot._submittedUrls.List[e.Message.Id].GuildOrigin, new PhishingSubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await _bot._databaseClient._helper.DeleteRow(_bot._databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _bot._submittedUrls.List.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }

            }
        });
    }
}
