// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

internal sealed class PhishingSubmissionEvents
{
    internal PhishingSubmissionEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (this._bot.submittedUrls.ContainsKey(e.Message?.Id ?? 0))
        {
            if (!e.User.IsMaintenance(this._bot.status))
                return;

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == "accept_submission")
            {
                this._bot.phishingUrls.Add(this._bot.submittedUrls[e.Message.Id].Url, new PhishingUrlEntry
                {
                    Origin = new(),
                    Submitter = this._bot.submittedUrls[e.Message.Id].Submitter,
                    Url = this._bot.submittedUrls[e.Message.Id].Url
                });

                this._bot.submittedUrls.Remove(e.Message.Id);

                try
                {
                    await this._bot.databaseClient._helper.DeleteRow(this._bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = e.Message.DeleteAsync();

                try
                {
                    _ = new PhishingUrlUpdater(this._bot).UpdateDatabase(new());
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to update database", ex);
                }
            }
            else if (e.GetCustomId() == "deny_submission")
            {
                this._bot.submittedUrls.Remove(e.Message.Id);

                try
                {
                    await this._bot.databaseClient._helper.DeleteRow(this._bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = e.Message.DeleteAsync();
            }
            else if (e.GetCustomId() == "ban_user")
            {
                this._bot.phishingUrlSubmissionUserBans.Add(this._bot.submittedUrls[e.Message.Id].Submitter, new PhishingSubmissionBanDetails
                {
                    Reason = "Too many denied requests | Manual ban",
                    Moderator = e.User.Id
                });

                try
                {
                    await this._bot.databaseClient._helper.DeleteRow(this._bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                this._bot.submittedUrls.Remove(e.Message.Id);

                _ = e.Message.DeleteAsync();
            }
            else if (e.GetCustomId() == "ban_guild")
            {
                this._bot.phishingUrlSubmissionGuildBans.Add(this._bot.submittedUrls[e.Message.Id].GuildOrigin, new PhishingSubmissionBanDetails
                {
                    Reason = "Too many denied requests | Manual ban",
                    Moderator = e.User.Id
                });

                try
                {
                    await this._bot.databaseClient._helper.DeleteRow(this._bot.databaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                this._bot.submittedUrls.Remove(e.Message.Id);

                _ = e.Message.DeleteAsync();
            }

        }
    }
}
