// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

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
            if (_bot.submittedUrls.ContainsKey(e.Message?.Id ?? 0))
            {
                if (!e.User.IsMaintenance(_bot.status))
                    return;

                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (e.GetCustomId() == "accept_submission")
                {
                    _bot.phishingUrls.Add(_bot.submittedUrls[e.Message.Id].Url, new PhishingUrlEntry
                    {
                        Origin = new(),
                        Submitter = _bot.submittedUrls[e.Message.Id].Submitter,
                        Url = _bot.submittedUrls[e.Message.Id].Url
                    });

                    _bot.submittedUrls.Remove(e.Message.Id);

                    try
                    {
                        await _bot.databaseClient._helper.DeleteRow(_bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();

                    try
                    {
                        _ = new PhishingUrlUpdater(_bot).UpdateDatabase(new());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to update database", ex);
                    }
                }
                else if (e.GetCustomId() == "deny_submission")
                {
                    _bot.submittedUrls.Remove(e.Message.Id);

                    try
                    {
                        await _bot.databaseClient._helper.DeleteRow(_bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _ = e.Message.DeleteAsync();
                }
                else if (e.GetCustomId() == "ban_user")
                {
                    _bot.phishingUrlSubmissionUserBans.Add(_bot.submittedUrls[e.Message.Id].Submitter, new PhishingSubmissionBanDetails
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await _bot.databaseClient._helper.DeleteRow(_bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _bot.submittedUrls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }
                else if (e.GetCustomId() == "ban_guild")
                {
                    _bot.phishingUrlSubmissionGuildBans.Add(_bot.submittedUrls[e.Message.Id].GuildOrigin, new PhishingSubmissionBanDetails
                    {
                        Reason = "Too many denied requests | Manual ban",
                        Moderator = e.User.Id
                    });

                    try
                    {
                        await _bot.databaseClient._helper.DeleteRow(_bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                    }
                    catch { }

                    _bot.submittedUrls.Remove(e.Message.Id);

                    _ = e.Message.DeleteAsync();
                }

            }
        });
    }
}
