namespace Project_Ichigo.Events;

internal class CrosspostEvents
{
    internal CrosspostEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            foreach (var b in _bot._guilds.Servers[e.Guild.Id].CrosspostChannels.ToList())
                if (!e.Guild.Channels.ContainsKey(b))
                    _bot._guilds.Servers[e.Guild.Id].CrosspostChannels.Remove(b);

            if (_bot._guilds.Servers[e.Guild.Id].CrosspostChannels.Contains(e.Channel.Id))
            {
                if (e.Channel.Type != ChannelType.News)
                {
                    bool ReactionAdded = false;

                    var task = e.Channel.CrosspostMessageAsync(e.Message).ContinueWith(s =>
                    {
                        if (ReactionAdded)
                            _ = e.Message.DeleteOwnReactionAsync(DiscordEmoji.FromGuildEmote(sender, 940100205720784936));
                    });

                    await Task.Delay(5000);

                    if (!task.IsCompleted)
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 940100205720784936));
                        ReactionAdded = true;
                    }
                }
            }
        }).Add(_bot._watcher);
    }
}
