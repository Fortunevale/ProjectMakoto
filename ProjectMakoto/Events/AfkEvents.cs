// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;
internal sealed class AfkEvents : RequiresTranslation
{
    internal AfkEvents(Bot bot) : base(bot)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (this.Bot.objectedUsers.Contains(e.Author.Id) || this.Bot.bannedUsers.ContainsKey(e.Author.Id) || this.Bot.bannedGuilds.ContainsKey(e.Guild?.Id ?? 0))
            return;

        var prefix = e.Guild.GetGuildPrefix(this.Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e.Guild == null || e.Channel.IsPrivate || e.Author.IsBot)
            return;

        var AfkKey = this.Bot.LoadedTranslations.Commands.Social.Afk;

        if (this.Bot.Users[e.Author.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch && this.Bot.Users[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
        {
            var cache = new DateTime().ToUniversalTime().AddTicks(this.Bot.Users[e.Author.Id].AfkStatus.TimeStamp.Ticks);

            this.Bot.Users[e.Author.Id].AfkStatus.Reason = "";
            this.Bot.Users[e.Author.Id].AfkStatus.TimeStamp = DateTime.UnixEpoch;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(this.Bot.Users[e.Author.Id])} • {e.Guild.Name}" },
                Color = EmbedColors.Info,
                Timestamp = DateTime.UtcNow,
                Description = AfkKey.Events.NoLongerAfk.Get(this.Bot.Users[e.Author.Id]).Build(true,
                new TVar("User", e.Author.Mention),
                new TVar("Timestamp", cache.ToTimestamp()))
            };

            var ExtendDelay = false;

            if (this.Bot.Users[e.Author.Id].AfkStatus.MessagesAmount > 0)
            {
                embed.Description += $"\n\n{AfkKey.Events.NoLongerAfk.Get(this.Bot.Users[e.Author.Id]).Build(true)}\n" +
                                        $"{string.Join("\n", this.Bot.Users[e.Author.Id].AfkStatus.Messages
                                            .Select(x => AfkKey.Events.MessageListing
                                                .Get(this.Bot.Users[e.Author.Id])
                                                .Build(true,
                                                new TVar("User", $"<@!{x.AuthorId}>"),
                                                new TVar("Message", new EmbeddedLink($"https://discord.com/channels/{x.GuildId}/{x.ChannelId}/{x.MessageId}", AfkKey.Events.Message.Get(this.Bot.Users[e.Author.Id]))))))}";

                ExtendDelay = true;

                if (this.Bot.Users[e.Author.Id].AfkStatus.MessagesAmount - 5 > 0)
                {
                    embed.Description += $"\n{AfkKey.Events.AndMore.Get(this.Bot.Users[e.Author.Id])
                        .Build(true, new TVar("Count", this.Bot.Users[e.Author.Id].AfkStatus.MessagesAmount - 5))}";
                }

                this.Bot.Users[e.Author.Id].AfkStatus.MessagesAmount = 0;
                this.Bot.Users[e.Author.Id].AfkStatus.Messages = new();
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

                if (this.Bot.Users[b.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch)
                {
                    if (this.Bot.Users[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(30) > DateTime.UtcNow)
                        return;

                    if (this.Bot.Users[b.Id].AfkStatus.Messages.Count < 5)
                    {
                        this.Bot.Users[b.Id].AfkStatus.Messages.Add(new()
                        {
                            AuthorId = e.Author.Id,
                            ChannelId = e.Channel.Id,
                            GuildId = e.Guild.Id,
                            MessageId = e.Message.Id,
                        });
                    }

                    this.Bot.Users[b.Id].AfkStatus.MessagesAmount++;

                    this.Bot.Users[e.Author.Id].AfkStatus.LastMentionTrigger = DateTime.UtcNow;

                    _ = e.Message.RespondAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(this.Bot.Users[e.Author.Id])} • {e.Guild.Name}" },
                        Color = EmbedColors.Info,
                        Timestamp = DateTime.UtcNow,
                        Description = AfkKey.Events.CurrentlyAfk.Get(this.Bot.Users[e.Author.Id]).Build(true,
                            new TVar("User", b.Mention),
                            new TVar("Timestamp", this.Bot.Users[b.Id].AfkStatus.TimeStamp.ToTimestamp()),
                            new TVar("Reason", this.Bot.Users[b.Id].AfkStatus.Reason.FullSanitize()))
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
