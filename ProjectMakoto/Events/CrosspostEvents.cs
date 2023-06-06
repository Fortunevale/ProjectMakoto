// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class CrosspostEvents
{
    internal CrosspostEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Guild is null || e.Channel.IsPrivate)
            return;

        foreach (var b in _bot.guilds[e.Guild.Id].Crosspost.CrosspostChannels.ToList())
            if (!e.Guild.Channels.ContainsKey(b))
                _bot.guilds[e.Guild.Id].Crosspost.CrosspostChannels.Remove(b);

        if (!_bot.guilds[e.Guild.Id].Crosspost.CrosspostChannels.Contains(e.Channel.Id))
            return;

        if (e.Message.Reference is not null || e.Message.MessageType is MessageType.ChannelPinnedMessage or MessageType.GuildMemberJoin or MessageType.ChannelFollowAdd or MessageType.ChatInputCommand or MessageType.ContextMenuCommand)
            return;

        if (e.Channel.Type == ChannelType.News)
        {
            if (_bot.guilds[e.Guild.Id].Crosspost.ExcludeBots)
                if (e.Message.WebhookMessage || e.Message.Author.IsBot)
                    return;

            ulong MessageId = e.Message.Id;

            if (_bot.guilds[e.Guild.Id].Crosspost.DelayBeforePosting > 3)
                _ = e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🕒"));

            await Task.Delay(TimeSpan.FromSeconds(_bot.guilds[e.Guild.Id].Crosspost.DelayBeforePosting));

            if (_bot.guilds[e.Guild.Id].Crosspost.DelayBeforePosting > 3)
                _ = e.Message.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode("🕒"));

            DiscordMessage msg;

            try
            {
                msg = await e.Channel.GetMessageAsync(e.Message.Id);
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                return;
            }
            catch (Exception)
            {
                throw;
            }

            bool ReactionAdded = false;

            var task = _bot.guilds[e.Guild.Id].Crosspost.CrosspostWithRatelimit(e.Channel, e.Message).ContinueWith(s =>
            {
                if (ReactionAdded)
                    _ = msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
            });

            await Task.Delay(5000);

            if (!task.IsCompleted)
            {
                if (!ReactionAdded)
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                    ReactionAdded = true; 
                }
            }
        }
    }
}
