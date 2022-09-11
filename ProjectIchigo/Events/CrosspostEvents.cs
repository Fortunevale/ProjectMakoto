namespace ProjectIchigo.Events;

internal class CrosspostEvents
{
    internal CrosspostEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild is null || e.Channel.IsPrivate)
                return;

            if (!_bot.guilds.ContainsKey(e.Guild.Id))
                _bot.guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            foreach (var b in _bot.guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                if (!e.Guild.Channels.ContainsKey(b))
                    _bot.guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

            if (!_bot.guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(e.Channel.Id))
                return;

            if (e.Message.Reference is not null || e.Message.MessageType is MessageType.ChannelPinnedMessage or MessageType.GuildMemberJoin or MessageType.ChannelFollowAdd or MessageType.ChatInputCommand or MessageType.ContextMenuCommand)
                return;

            if (e.Channel.Type == ChannelType.News)
            {
                if (_bot.guilds[e.Guild.Id].CrosspostSettings.ExcludeBots)
                    if (e.Message.WebhookMessage || e.Message.Author.IsBot)
                        return;

                ulong MessageId = e.Message.Id;

                if (_bot.guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting > 3)
                    _ = e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🕒"));

                await Task.Delay(TimeSpan.FromSeconds(_bot.guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting));

                if (_bot.guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting > 3)
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

                var task = _bot.guilds[e.Guild.Id].CrosspostSettings.CrosspostWithRatelimit(e.Channel, e.Message).ContinueWith(s =>
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
        }).Add(_bot.watcher);
    }
}
