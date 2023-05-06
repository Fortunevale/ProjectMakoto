// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

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
                _logger.LogInfo("Leaving guild '{Guild}'..", e.Guild.Id);
                await e.Guild.LeaveAsync();
                return;
            }

            DiscordChannel channel;

            try
            {
                if (e.Guild.SystemChannel is null)
                    channel = e.Guild.Channels.Values.OrderBy(x => x.Position).First(x => x.Type == ChannelType.Text && x.Id != x.Guild.RulesChannel?.Id);
                else
                    channel = e.Guild.SystemChannel;
            }
            catch (Exception) { return; }

            if (sender.Guilds.Count >= 100 && (!sender.CurrentUser.IsVerifiedBot || !_bot.status.LoadedConfig.AllowMoreThan100Guilds))
            {
                await channel.SendMessageAsync($"Hi, thanks for adding me to your server.\n\n" +
                    $"Unfortunately, I am not yet verified.\n\nBecause i need several intents (read more about that here: <https://support.discord.com/hc/en-us/articles/360040720412>) like the server members and message content, " +
                    $"i am unable to operate in more than 99 servers.\nTo see how my verification is going, check our development and support server: <{_bot.status.DevelopmentServerInvite}>.");
                
                await Task.Delay(1000);
                await e.Guild.LeaveAsync();
                return;
            }

            var msg = await channel.SendMessageAsync(
                $"Hi! I'm Makoto. I support Slash Commands, but additionally you can use me via `;;`. To get a list of all commands, type {sender.GetCommandMention(_bot, "help")}.\n\n" +
                $"**Important Notes**\n\n" +
                $"• **Phishing Protection** is **enabled** by default. To change this run: {sender.GetCommandMention(_bot, "phishing")}.\n" +
                $"• **Automatic User/Bot Token invalidation** is **turned on** by default. If you don't know what this means, just leave it on. If you do know what this means and you don't want it to happen, run {sender.GetCommandMention(_bot, "tokendetection")}.\n" +
                $"• Every server is opted into a global ban system. When someone is known to break Discord's TOS, us bot staff can quickly scoop them up and ban them even before their account gets terminated by Discord. You can opt out via {sender.GetCommandMention(_bot, "join")}.\n\n" +
                $"If you need help, feel free to join our Support and Development Server: <{_bot.status.DevelopmentServerInvite}>\n" +
                $"To find out more about me, check my Github Repo: <https://s.aitsys.dev/makoto>.\n\n" +
                $"_This message will automatically be deleted {DateTime.UtcNow.AddMinutes(60).ToTimestamp()}._");

            new Task(async () =>
            {
                _ = msg.DeleteAsync();
            }).CreateScheduleTask(DateTime.UtcNow.AddMinutes(60));
        }).Add(_bot.watcher);
    }
}
