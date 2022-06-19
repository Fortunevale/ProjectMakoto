namespace ProjectIchigo;

internal class VoicePrivacyEvents
{
    internal VoicePrivacyEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (_bot._guilds.List[e.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled)
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
            }).Add(_bot._watcher);
        }

        if (_bot._guilds.List[e.Guild.Id].InVoiceTextPrivacySettings.ClearTextEnabled)
        {
            Task.Run(async () =>
            {
                if (e.After?.Channel?.Id != e.Before?.Channel?.Id)
                {
                    if (e.Before is not null && e.Before.Channel is not null)
                    {
                        List<DiscordMessage> discordMessages = new();
                        discordMessages.AddRange(await e.Before.Channel.GetMessagesAsync(1));

                        int failcount = 0;

                        while (true)
                        {
                            try
                            {
                                var requestedMsgs = await e.Before.Channel.GetMessagesBeforeAsync(discordMessages.Last().Id, 100);

                                if (!requestedMsgs.Any())
                                    break;

                                discordMessages.AddRange(requestedMsgs);
                                await Task.Delay(10000);
                            }
                            catch (Exception ex) 
                            { 
                                _logger.LogWarn($"Failed to get messages for in voice text clearer", ex);

                                await Task.Delay(30000);
                                failcount++;

                                if (failcount >= 3)
                                    break;
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
                                    await e.Before.Channel.DeleteMessagesAsync(MessagesToDelete);
    
                                    for (int i = 0; i < MessagesToDelete.Count; i++)
                                    {
                                        BulkDeletions.Remove(MessagesToDelete[i]);
                                    }
                                    await Task.Delay(30000);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarn($"Failed to bulk delete messages for in voice text clearer", ex);

                                    await Task.Delay(30000);
                                    failcount++;

                                    if (failcount >= 3)
                                        break;
                                }
                            }

                            failcount = 0;
                            var SingleDeletions = discordMessages.Where(x => x.Timestamp.GetTimespanSince() > TimeSpan.FromDays(14)).ToList();

                            while (BulkDeletions.Count > 0)
                            {
                                try
                                {
                                    var msg = SingleDeletions[0];

                                    await e.Before.Channel.DeleteMessageAsync(msg);
                                    SingleDeletions.Remove(msg);
                                    await Task.Delay(30000);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarn($"Failed to single delete messages for in voice text clearer", ex);

                                    await Task.Delay(30000);
                                    failcount++;

                                    if (failcount >= 3)
                                        break;
                                }
                            }
                        }
                    }
                }
            }).Add(_bot._watcher);
        }
    }

    internal async Task ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (_bot._guilds.List[e.Guild.Id].InVoiceTextPrivacySettings.SetPermissionsEnabled)
            {
                if (!e.Guild.Channels.Any(x => x.Value.Type == ChannelType.Voice))
                    return;

                foreach (var b in e.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice))
                {
                    _ = b.Value.AddOverwriteAsync(e.Guild.EveryoneRole, Permissions.None, Permissions.ReadMessageHistory | Permissions.SendMessages, "In-Voice Privacy is enabled");
                }
            }
        }).Add(_bot._watcher);
    }
}