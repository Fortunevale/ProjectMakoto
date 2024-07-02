// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class DiscordEvents(Bot bot) : RequiresTranslation(bot)
{
    Translations.events.genericEvent tKey
        => this.Bot.LoadedTranslations.Events.GenericEvent;

    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        if (this.Bot.objectedUsers.Contains(e.Guild.OwnerId) || this.Bot.bannedUsers.ContainsKey(e.Guild.OwnerId) || this.Bot.bannedGuilds.ContainsKey(e.Guild?.Id ?? 0))
        {
            await Task.Delay(1000);
            Log.Information("Leaving guild '{Guild}'..", e.Guild.Id);
            await e.Guild.LeaveAsync();
            return;
        }

        DiscordChannel channel;

        try
        {
            channel = e.Guild.SystemChannel is null
                ? e.Guild.Channels.Values.OrderBy(x => x.Position).First(x => x.Type == ChannelType.Text && x.Id != x.Guild.RulesChannel?.Id)
                : e.Guild.SystemChannel;
        }
        catch (Exception) { return; }

        if (sender.Guilds.Count >= 100 && (!sender.CurrentUser.IsVerifiedBot || !this.Bot.status.LoadedConfig.AllowMoreThan100Guilds))
        {
            _ = await channel.SendMessageAsync(this.tKey.LimitedReached.Get(this.Bot.Guilds[e.Guild.Id]).Build(
                new TVar("IntentsUrl", "<https://support.discord.com/hc/en-us/articles/360040720412>"),
                new TVar("Invite", $"<{this.Bot.status.DevelopmentServerInvite}>")));

            await Task.Delay(1000);
            await e.Guild.LeaveAsync();
            return;
        }

        var msg = await channel.SendMessageAsync(this.tKey.SuccessfulJoin.Get(this.Bot.Guilds[e.Guild.Id]).Build(false, true,
            new TVar("Bot", sender.CurrentUser.GetUsername()),
            new TVar("BotMention", sender.CurrentUser.Mention),
            new TVar("Help", sender.GetCommandMention(this.Bot, "help")),
            new TVar("Phishing", "`/config phishing`"),
            new TVar("TokenDetection", "`/config tokendetection`"),
            new TVar("Join", "`/config join`"),
            new TVar("Invite", $"<{this.Bot.status.DevelopmentServerInvite}>"),
            new TVar("GithubRepo", "<https://s.aitsys.dev/makoto>"),
            new TVar("Timestamp", DateTime.UtcNow.AddMinutes(60).ToTimestamp())));

        _ = new Func<Task>(async () =>
        {
            _ = msg.DeleteAsync();
        }).CreateScheduledTask(DateTime.UtcNow.AddMinutes(60));
    }
}
