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
            if (_bot.objectedUsers.Contains(e.Guild.OwnerId) || _bot.bannedUsers.ContainsKey(e.Guild.OwnerId) || _bot.bannedGuilds.ContainsKey(e.Guild?.Id ?? 0))
            {
                await Task.Delay(1000);
                _logger.LogInfo($"Leaving guild '{e.Guild.Id}'..");
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

            if (sender.Guilds.Count >= 100 && (!sender.CurrentUser.IsVerifiedBot || !_bot.status.LoadedConfig.AllowMoreThan100Guilds))
            {
                await e.Guild.Channels.Values.OrderBy(x => x.Position).First().SendMessageAsync($"Hi, thanks for adding me to your server.\n\n" +
                    $"Unfortunately, I am not yet verified.\n\nBecause i need several intents (read more about that here: <https://support.discord.com/hc/en-us/articles/360040720412>) like the server members and message content, " +
                    $"i am unable to operate in more than 99 servers.\nTo see how my verification is going, check our development and support server: <{_bot.status.DevelopmentServerInvite}>.");
                
                await Task.Delay(1000);
                await e.Guild.LeaveAsync();
                return;
            }

            var msg = await e.Guild.Channels.Values.OrderBy(x => x.Position).First().SendMessageAsync(
                $"Hi! I'm Ichigo. I support Slash Commands, but additionally you can use me via `;;`. To get a list of all commands, type `;;help` or do a `/` and filter by me.\n\n" +
                $"**Important Notes**\n\n" +
                $"• **Phishing Protection** is **enabled** by default. To change this run: {sender.GetCommandMention(_bot, "phishing")}.\n" +
                $"• **Automatic User/Bot Token invalidation** is **turned on** by default. If you don't know what this means, just leave it on. If you do know what this means and you don't want it to happen, run {sender.GetCommandMention(_bot, "tokendetectionsettings")}.\n" +
                $"• Every server is opted into a global ban system. When someone is known to break Discord's TOS, us bot staff can quickly scoop them up and ban them even before their account gets terminated by Discord. You can opt out via {sender.GetCommandMention(_bot, "join")}.\n\n" +
                $"If you need help, feel free to join our Support and Development Server: <{_bot.status.DevelopmentServerInvite}>\n" +
                $"To find out more about me, check my Github Repo: <https://s.aitsys.dev/ichigo>.\n\n" +
                $"_This message will automatically be deleted {DateTime.UtcNow.AddMinutes(60).ToTimestamp()}._");

            new Task(async () =>
            {
                _ = msg.DeleteAsync();
            }).CreateScheduleTask(DateTime.UtcNow.AddMinutes(60));
        }).Add(_bot.watcher);
    }
}
