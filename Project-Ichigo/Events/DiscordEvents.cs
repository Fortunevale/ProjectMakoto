namespace Project_Ichigo.Events;

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
            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            foreach (var guild in sender.Guilds)
            {
                if (!_bot._guilds.Servers.ContainsKey(guild.Key))
                    _bot._guilds.Servers.Add(guild.Key, new ServerInfo.ServerSettings());
            }
        }).Add(_bot._watcher);
    }
}
