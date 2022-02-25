namespace Project_Ichigo.PhishingProtection;

internal class SubmissionEvents
{
    internal SubmissionEvents(MySqlConnection databaseConnection, DatabaseHelper databaseHelper, SubmittedUrls _submittedUrls, PhishingUrls _phishingUrls, Status _status, SubmissionBans _submissionBans)
    {
        this.databaseConnection = databaseConnection;
        this.databaseHelper = databaseHelper;
        this._submittedUrls = _submittedUrls;
        this._phishingUrls = _phishingUrls;
        this._status = _status;
        this._submissionBans = _submissionBans;
    }

    internal MySqlConnection databaseConnection { private get; set; }
    internal DatabaseHelper databaseHelper { private get; set; }
    internal SubmittedUrls _submittedUrls { private get; set; }
    internal PhishingUrls _phishingUrls { private get; set; }
    internal Status _status { private get; set; }
    internal SubmissionBans _submissionBans { private get; set; }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            if (_submittedUrls.Urls.ContainsKey(e.Message.Id))
            {
                if (!e.User.IsMaintenance(_status))
                    return;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.Interaction.Data.CustomId == "accept_submission")
                {
                    _phishingUrls.List.Add(_submittedUrls.Urls[e.Message.Id].Url, new PhishingUrls.UrlInfo
                    {
                        Origin = new(),
                        Submitter = _submittedUrls.Urls[e.Message.Id].Submitter,
                        Url = _submittedUrls.Urls[e.Message.Id].Url
                    });

                    _submittedUrls.Urls.Remove(e.Message.Id);

                    try
                    {
                        await databaseHelper.DeleteRow("active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();

                    try
                    {
                        _ = new PhishingUrlUpdater(databaseConnection, databaseHelper).UpdateDatabase(_phishingUrls, new());
                    }
                    catch (Exception ex)
                    {
                        LogError($"{ex}");
                    }
                }
                else if (e.Interaction.Data.CustomId == "deny_submission")
                {
                    _submittedUrls.Urls.Remove(e.Message.Id);

                    try
                    {
                        await databaseHelper.DeleteRow("active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_user")
                {
                    _submissionBans.BannedUsers.Add(_submittedUrls.Urls[e.Message.Id].Submitter, new SubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await databaseHelper.DeleteRow("active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _submittedUrls.Urls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }
                else if (e.Interaction.Data.CustomId == "ban_guild")
                {
                    _submissionBans.BannedGuilds.Add(_submittedUrls.Urls[e.Message.Id].GuildOrigin, new SubmissionBans.BanInfo
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await databaseHelper.DeleteRow("active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _submittedUrls.Urls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }

            }
        });
    }
}
