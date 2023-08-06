// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

internal sealed class PhishingSubmissionEvents : RequiresBotReference
{
    public PhishingSubmissionEvents(Bot bot) : base(bot)
    {
    }

    internal async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
    {
        if (this.Bot.SubmittedHosts.ContainsKey(e.Message?.Id ?? 0))
        {
            if (!e.User.IsMaintenance(this.Bot.status))
                return;

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (e.GetCustomId() == "accept_submission")
            {
                this.Bot.PhishingHosts.Add(this.Bot.SubmittedHosts[e.Message.Id].Url, new PhishingUrlEntry
                {
                    Origin = new(),
                    Submitter = this.Bot.SubmittedHosts[e.Message.Id].Submitter,
                    Url = this.Bot.SubmittedHosts[e.Message.Id].Url
                });

                _ = this.Bot.SubmittedHosts.Remove(e.Message.Id);

                try
                {
                    await this.Bot.DatabaseClient._helper.DeleteRow(this.Bot.DatabaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = e.Message.DeleteAsync();

                try
                {
                    _ = new PhishingUrlHandler(this.Bot).UpdateDatabase(new());
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to update database", ex);
                }
            }
            else if (e.GetCustomId() == "deny_submission")
            {
                _ = this.Bot.SubmittedHosts.Remove(e.Message.Id);

                try
                {
                    await this.Bot.DatabaseClient._helper.DeleteRow(this.Bot.DatabaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = e.Message.DeleteAsync();
            }
            else if (e.GetCustomId() == "ban_user")
            {
                this.Bot.bannedUsers.Add(this.Bot.SubmittedHosts[e.Message.Id].Submitter, new BanDetails
                {
                    Reason = "Too many invalid reported hosts | Manual ban",
                    Moderator = e.User.Id
                });

                try
                {
                    await this.Bot.DatabaseClient._helper.DeleteRow(this.Bot.DatabaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = this.Bot.SubmittedHosts.Remove(e.Message.Id);

                _ = e.Message.DeleteAsync();
            }
            else if (e.GetCustomId() == "ban_guild")
            {
                this.Bot.bannedGuilds.Add(this.Bot.SubmittedHosts[e.Message.Id].GuildOrigin, new BanDetails
                {
                    Reason = "Too many invalid reported hosts | Manual ban",
                    Moderator = e.User.Id
                });

                try
                {
                    await this.Bot.DatabaseClient._helper.DeleteRow(this.Bot.DatabaseClient.mainDatabaseConnection, "active_url_submissions", "messageid", $"{e.Message.Id}");
                }
                catch { }

                _ = this.Bot.SubmittedHosts.Remove(e.Message.Id);

                _ = e.Message.DeleteAsync();
            }
        }
    }
}
