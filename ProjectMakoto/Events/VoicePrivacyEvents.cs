// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

internal sealed class VoicePrivacyEvents
{
    internal VoicePrivacyEvents(Bot _bot)
    {
        this._bot = _bot;
        QueueHandler();
    }

    public Bot _bot { private get; set; }

    private List<Task> JobsQueue = new();

    internal async void QueueHandler()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    while (this.JobsQueue.Count <= 0)
                        Thread.Sleep(1000);

                    var task = this.JobsQueue[0];
                    this.JobsQueue.Remove(task);

                    task.Start();
                    task.Add(this._bot);
                }
                catch (Exception ex)
                {
                    _logger.LogWarn("Failed to run queue item", ex);
                }
            }
        }).Add(this._bot);
    }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (this._bot.guilds[e.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled)
        {
            Task.Run(async () =>
            {
                if (e.After?.Channel?.Id != e.Before?.Channel?.Id)
                {
                    if (e.Before is not null && e.Before.Channel is not null)
                        await e.Before.Channel.DeleteOverwriteAsync(await e.User.ConvertToMember(e.Guild), "Left VC while In-Voice Privacy Set permissions is turned on");

                    if (e.After is not null && e.After.Channel is not null)
                        await e.After?.Channel?.AddOverwriteAsync(await e.User.ConvertToMember(e.Guild), Permissions.ReadMessageHistory | Permissions.SendMessages, Permissions.None, "Joined VC while In-Voice Privacy Set permissions is turned on");
                }
            }).Add(this._bot);
        }

        if (this._bot.guilds[e.Guild.Id].InVoiceTextPrivacy.ClearTextEnabled)
        {
            this.JobsQueue.Add(new Task(async () =>
            {
                try
                {
                    if (e.After?.Channel?.Id != e.Before?.Channel?.Id)
                    {
                        if (e.Before is not null && e.Before.Channel is not null)
                        {
                            if (e.Before.Channel.Type != ChannelType.Voice)
                                return;

                            List<DiscordMessage> discordMessages = new();
                            discordMessages.AddRange(await e.Before.Channel.GetMessagesAsync(1));

                            if (!discordMessages.Any())
                                return;

                            int failcount = 0;

                            while (true)
                            {
                                try
                                {
                                    var requestedMsgs = await e.Before.Channel.GetMessagesBeforeAsync(discordMessages.Last().Id, 100);

                                    if (!requestedMsgs.Any())
                                        break;

                                    discordMessages.AddRange(requestedMsgs);

                                    if (requestedMsgs.Any())
                                        await Task.Delay(10000);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarn("An exception occurred while trying to get messages from a channel ({failcount}/{max})", ex, failcount, 3);

                                    await Task.Delay(10000);
                                    failcount++;

                                    if (failcount >= 3)
                                        throw;
                                }
                            }

                            discordMessages = discordMessages.Where(x => x.Author.Id == e.User.Id).ToList();

                            if (discordMessages.Any())
                            {
                                failcount = 0;
                                var BulkDeletions = discordMessages.Where(x => x.Timestamp.GetTimespanSince() < TimeSpan.FromDays(14)).ToList();

                                while (BulkDeletions.Count > 0)
                                {
                                    try
                                    {
                                        var MessagesToDelete = BulkDeletions.Take(100).ToList();
                                        await e.Before.Channel.DeleteMessagesAsync(MessagesToDelete, "In Voice Privacy");

                                        for (int i = 0; i < MessagesToDelete.Count; i++)
                                        {
                                            BulkDeletions.Remove(MessagesToDelete[i]);
                                        }

                                        if (BulkDeletions.Any())
                                            await Task.Delay(30000);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarn("An exception occurred while trying to bulk delete messages from a channel ({failcount}/{max})", ex, failcount, 3);

                                        await Task.Delay(30000);
                                        failcount++;

                                        if (failcount >= 3)
                                            throw;
                                    }
                                }

                                failcount = 0;
                                var SingleDeletions = discordMessages.Where(x => x.Timestamp.GetTimespanSince() > TimeSpan.FromDays(14)).ToList();

                                while (SingleDeletions.Count > 0)
                                {
                                    try
                                    {
                                        var msg = SingleDeletions[0];

                                        await e.Before.Channel.DeleteMessageAsync(msg, "In Voice Privacy");
                                        SingleDeletions.Remove(msg);

                                        if (SingleDeletions.Any())
                                            await Task.Delay(30000);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarn("An exception occurred while trying to delete a message from a channel ({failcount}/{max})", ex, failcount, 3);

                                        await Task.Delay(30000);
                                        failcount++;

                                        if (failcount >= 3)
                                            throw;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (DisCatSharp.Exceptions.NotFoundException) { }
                catch (DisCatSharp.Exceptions.UnauthorizedException) { }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to execute a In-Voice Text Privacy Cleaner", ex);
                }
            }));
        }
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (this._bot.guilds[e.Guild.Id].InVoiceTextPrivacy.SetPermissionsEnabled)
            {
                if (!e.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                    return;

                foreach (var b in e.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                {
                    DiscordOverwrite present = null;
                    if (b.Value.Parent?.PermissionOverwrites.Any(x => (x.Type == OverwriteType.Role) && (x.Id == e.Guild.EveryoneRole.Id)) ?? false)
                        present = b.Value.Parent.PermissionOverwrites.First(x => (x.Type == OverwriteType.Role) && (x.Id == e.Guild.EveryoneRole.Id));

                    _ = b.Value.AddOverwriteAsync(e.Guild.EveryoneRole, (present?.Allowed ?? Permissions.None), (present?.Denied ?? Permissions.None) | Permissions.ReadMessageHistory | Permissions.SendMessages, "In-Voice Privacy is enabled");
                }
            }
        }).Add(this._bot);
    }
}