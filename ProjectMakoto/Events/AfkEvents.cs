// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;
internal class AfkEvents
{
    internal AfkEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (_bot.objectedUsers.Contains(e.Author.Id) || _bot.bannedUsers.ContainsKey(e.Author.Id) || _bot.bannedGuilds.ContainsKey(e.Guild?.Id ?? 0))
                return;

            if (e.Guild == null || e.Channel.IsPrivate || e.Message.Content.StartsWith(">>") || e.Message.Content.StartsWith(";;") || e.Author.IsBot)
                return;

            if (_bot.users[e.Author.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch && _bot.users[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
            {
                DateTime cache = new DateTime().ToUniversalTime().AddTicks(_bot.users[e.Author.Id].AfkStatus.TimeStamp.Ticks);

                _bot.users[e.Author.Id].AfkStatus.Reason = "";
                _bot.users[e.Author.Id].AfkStatus.TimeStamp = DateTime.UnixEpoch;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                    Color = EmbedColors.Info,
                    Timestamp = DateTime.UtcNow,
                    Description = $"{e.Author.Mention} `You're no longer afk. You've been afk for {cache.GetTotalSecondsSince().GetHumanReadable()}.`"
                };

                bool ExtendDelay = false;

                if (_bot.users[e.Author.Id].AfkStatus.MessagesAmount > 0)
                {
                    embed.Description += $"\n\n`Here is what you missed:`\n" +
                                         $"{string.Join("\n", _bot.users[e.Author.Id].AfkStatus.Messages.Select(x => $"[`Message`](https://discord.com/channels/{x.GuildId}/{x.ChannelId}/{x.MessageId}) by <@!{x.AuthorId}>"))}";

                    ExtendDelay = true;

                    if (_bot.users[e.Author.Id].AfkStatus.MessagesAmount - 5 > 0)
                    {
                        embed.Description += $"\n`And {_bot.users[e.Author.Id].AfkStatus.MessagesAmount - 5} more..`";
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
                            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                            Color = EmbedColors.Info,
                            Timestamp = DateTime.UtcNow,
                            Description = $"{b.Mention} `is currently AFK and has been since` {Formatter.Timestamp(_bot.users[b.Id].AfkStatus.TimeStamp)}`, they most likely wont answer your message: '{_bot.users[b.Id].AfkStatus.Reason}'`"
                        }).ContinueWith(async x =>
                        {
                            await Task.Delay(10000);
                            _ = x.Result.DeleteAsync();
                        });
                        return;
                    }
                }
            }

        }).Add(_bot.watcher);
    }
}
