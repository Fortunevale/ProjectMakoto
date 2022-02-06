namespace Project_Ichigo.Events;

internal class DiscordEvents
{
    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        if (!Bot._guilds.Servers.ContainsKey(e.Guild.Id))
            Bot._guilds.Servers.Add(e.Guild.Id, new Settings.ServerSettings());

        foreach (var guild in sender.Guilds)
        {
            if (!Bot._guilds.Servers.ContainsKey(guild.Key))
                Bot._guilds.Servers.Add(guild.Key, new Settings.ServerSettings());
        }
    }
}
