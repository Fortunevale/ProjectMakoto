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
        }).Add(_bot._watcher);
    }
}
