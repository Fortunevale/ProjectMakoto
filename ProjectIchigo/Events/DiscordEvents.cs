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
        Task.Run(async () =>
        {
            if (_bot.ObjectedUsers.Contains(e.Guild.OwnerId))
            {
                await e.Guild.LeaveAsync();
                return;
            }

            if (!_bot._guilds.ContainsKey(e.Guild.Id))
                _bot._guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            foreach (var guild in sender.Guilds)
            {
                if (!_bot._guilds.ContainsKey(guild.Key))
                    _bot._guilds.Add(guild.Key, new Guild(guild.Key));
            }
        }).Add(_bot._watcher);
    }
}
