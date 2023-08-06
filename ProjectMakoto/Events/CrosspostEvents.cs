// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class CrosspostEvents : RequiresTranslation
{
    public CrosspostEvents(Bot bot) : base(bot)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Guild is null || e.Channel.IsPrivate)
            return;

        foreach (var b in this.Bot.Guilds[e.Guild.Id].Crosspost.CrosspostChannels.ToList())
            if (!e.Guild.Channels.ContainsKey(b))
                _ = this.Bot.Guilds[e.Guild.Id].Crosspost.CrosspostChannels.Remove(b);

        if (!this.Bot.Guilds[e.Guild.Id].Crosspost.CrosspostChannels.Contains(e.Channel.Id))
            return;

        if (e.Message.Reference is not null || e.Message.MessageType is MessageType.ChannelPinnedMessage or MessageType.GuildMemberJoin or MessageType.ChannelFollowAdd or MessageType.ChatInputCommand or MessageType.ContextMenuCommand)
            return;

        if (e.Channel.Type == ChannelType.News)
        {
            if (this.Bot.Guilds[e.Guild.Id].Crosspost.ExcludeBots)
                if (e.Message.WebhookMessage || e.Message.Author.IsBot)
                    return;

            if (this.Bot.Guilds[e.Guild.Id].Crosspost.DelayBeforePosting > 3)
                _ = e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🕒"));

            await Task.Delay(TimeSpan.FromSeconds(this.Bot.Guilds[e.Guild.Id].Crosspost.DelayBeforePosting));

            if (this.Bot.Guilds[e.Guild.Id].Crosspost.DelayBeforePosting > 3)
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

            await this.Bot.Guilds[e.Guild.Id].Crosspost.CrosspostWithRatelimit(sender, msg);
        }
    }
}
