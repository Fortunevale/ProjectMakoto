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
            if (_bot.objectedUsers.Contains(e.Guild.OwnerId))
            {
                await e.Guild.LeaveAsync();
                return;
            }

            if (!_bot.guilds.ContainsKey(e.Guild.Id))
                _bot.guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            foreach (var guild in sender.Guilds)
            {
                if (!_bot.guilds.ContainsKey(guild.Key))
                    _bot.guilds.Add(guild.Key, new Guild(guild.Key));
            }
        }).Add(_bot.watcher);
    }
}
