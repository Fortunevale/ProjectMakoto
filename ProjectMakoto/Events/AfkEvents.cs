// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;
internal sealed class AfkEvents
{
    internal AfkEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (_bot.objectedUsers.Contains(e.Author.Id) || _bot.bannedUsers.ContainsKey(e.Author.Id) || _bot.bannedGuilds.ContainsKey(e.Guild?.Id ?? 0))
            return;

        string prefix;

        try
        {
            prefix = _bot.guilds[e.Guild.Id].PrefixSettings.Prefix.IsNullOrWhiteSpace() ? ";;" : _bot.guilds[e.Guild.Id].PrefixSettings.Prefix;
        }
        catch (Exception)
        {
            prefix = ";;";
        }

        if (e.Message.Content.StartsWith(prefix))
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e.Guild == null || e.Channel.IsPrivate || e.Author.IsBot)
            return;

        var AfkKey = _bot.loadedTranslations.Commands.Social.Afk;

        if (_bot.users[e.Author.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch && _bot.users[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
        {
            DateTime cache = new DateTime().ToUniversalTime().AddTicks(_bot.users[e.Author.Id].AfkStatus.TimeStamp.Ticks);

            _bot.users[e.Author.Id].AfkStatus.Reason = "";
            _bot.users[e.Author.Id].AfkStatus.TimeStamp = DateTime.UnixEpoch;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(_bot.users[e.Author.Id])} • {e.Guild.Name}" },
                Color = EmbedColors.Info,
                Timestamp = DateTime.UtcNow,
                Description = AfkKey.Events.NoLongerAfk.Get(_bot.users[e.Author.Id]).Build(true, 
                new TVar("User", e.Author.Mention),
                new TVar("Timestamp", cache.ToTimestamp()))
            };

            bool ExtendDelay = false;

            if (_bot.users[e.Author.Id].AfkStatus.MessagesAmount > 0)
            {
                embed.Description += $"\n\n{AfkKey.Events.NoLongerAfk.Get(_bot.users[e.Author.Id]).Build(true)}\n" +
                                        $"{string.Join("\n", _bot.users[e.Author.Id].AfkStatus.Messages
                                            .Select(x => AfkKey.Events.MessageListing
                                                .Get(_bot.users[e.Author.Id])
                                                .Build(true, 
                                                new TVar("User", $"<@!{x.AuthorId}>"),
                                                new TVar("Message", $"[`{AfkKey.Events.Message.Get(_bot.users[e.Author.Id])}`](https://discord.com/channels/{x.GuildId}/{x.ChannelId}/{x.MessageId})"))))}";

                ExtendDelay = true;

                if (_bot.users[e.Author.Id].AfkStatus.MessagesAmount - 5 > 0)
                {
                    embed.Description += $"\n{AfkKey.Events.AndMore.Get(_bot.users[e.Author.Id])
                        .Build(true, new TVar("Count", _bot.users[e.Author.Id].AfkStatus.MessagesAmount - 5))}";
                }

                _bot.users[e.Author.Id].AfkStatus.MessagesAmount = 0;
                _bot.users[e.Author.Id].AfkStatus.Messages = new();
            }

            _ = e.Message.RespondAsync(embed).ContinueWith(async x =>
            {
                if (ExtendDelay)
                    await Task.Delay(30000);
                else
                    await Task.Delay(10000);

                _ = x.Result.DeleteAsync();
            });
        }

        if (e.MentionedUsers != null && e.MentionedUsers.Count > 0)
        {
            foreach (var b in e.MentionedUsers)
            {
                if (b.Id == e.Author.Id)
                    continue;

                if (_bot.users[b.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch)
                {
                    if (_bot.users[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(30) > DateTime.UtcNow)
                        return;

                    if (_bot.users[b.Id].AfkStatus.Messages.Count < 5)
                    {
                        _bot.users[b.Id].AfkStatus.Messages.Add(new MessageDetails
                        {
                            AuthorId = e.Author.Id,
                            ChannelId = e.Channel.Id,
                            GuildId = e.Guild.Id,
                            MessageId = e.Message.Id,
                        });
                    }

                    _bot.users[b.Id].AfkStatus.MessagesAmount++;

                    _bot.users[e.Author.Id].AfkStatus.LastMentionTrigger = DateTime.UtcNow;

                    _ = e.Message.RespondAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(_bot.users[e.Author.Id])} • {e.Guild.Name}" },
                        Color = EmbedColors.Info,
                        Timestamp = DateTime.UtcNow,
                        Description = AfkKey.Events.CurrentlyAfk.Get(_bot.users[e.Author.Id]).Build(true,
                            new TVar("User", b.Mention),
                            new TVar("Timestamp", _bot.users[b.Id].AfkStatus.TimeStamp.ToTimestamp()),
                            new TVar("Reason", _bot.users[b.Id].AfkStatus.Reason.FullSanitize()))
                    }).ContinueWith(async x =>
                    {
                        await Task.Delay(10000);
                        _ = x.Result.DeleteAsync();
                    });
                    return;
                }
            }
        }
    }
}
