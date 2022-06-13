namespace ProjectIchigo.Events;

internal class DiscordEvents
{
    internal DiscordEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }



    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(() =>
        {
            if (!_bot._guilds.List.ContainsKey(e.Guild.Id))
                _bot._guilds.List.Add(e.Guild.Id, new Guilds.ServerSettings());

            foreach (var guild in sender.Guilds)
            {
                if (!_bot._guilds.List.ContainsKey(guild.Key))
                    _bot._guilds.List.Add(guild.Key, new Guilds.ServerSettings());
            }

            foreach (var b in _bot._guilds.List)
                try
                {
                    b.Value.ProcessedAuditLogs.CollectionChanged -= _bot._collectionUpdates.AuditLogCollectionUpdated(b);
                    b.Value.CrosspostSettings.CrosspostChannels.CollectionChanged -= _bot._collectionUpdates.CrosspostCollectionUpdated(b);
                }
                catch { }

            foreach (var b in _bot._guilds.List)
            {
                b.Value.CrosspostSettings.CrosspostChannels.CollectionChanged += _bot._collectionUpdates.CrosspostCollectionUpdated(b);
                b.Value.ProcessedAuditLogs.CollectionChanged += _bot._collectionUpdates.AuditLogCollectionUpdated(b);
            }
        }).Add(_bot._watcher);
    }
}
