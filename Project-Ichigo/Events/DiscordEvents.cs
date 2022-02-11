namespace Project_Ichigo.Events;

internal class DiscordEvents
{
    internal Settings _guilds { private set; get; }



    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        if (!_guilds.Servers.ContainsKey(e.Guild.Id))
            _guilds.Servers.Add(e.Guild.Id, new Settings.ServerSettings());

        foreach (var guild in sender.Guilds)
        {
            if (!_guilds.Servers.ContainsKey(guild.Key))
                _guilds.Servers.Add(guild.Key, new Settings.ServerSettings());
        }
    }
}
