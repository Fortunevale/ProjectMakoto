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

            if (!_bot._guilds.ContainsKey(e.Guild.Id))
                _bot._guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            foreach (var b in _bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.ToList())
                if (!e.Guild.Channels.ContainsKey(b))
                    _bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.Remove(b);

            if (!_bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostChannels.Contains(e.Channel.Id))
                return;

            if (e.Channel.Type == ChannelType.News)
            {
                if (_bot._guilds[e.Guild.Id].CrosspostSettings.ExcludeBots)
                    if (e.Message.WebhookMessage || e.Message.Author.IsBot)
                        return;

                ulong MessageId = e.Message.Id;

                _bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostTasks.Add(new CrosspostMessage
                {
                    MessageId = e.Message.Id,
                    ChannelId = e.Channel.Id
                });

                if (_bot._guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting > 3)
                    _ = e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🕒"));

                await Task.Delay(TimeSpan.FromSeconds(_bot._guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting));

                if (_bot._guilds[e.Guild.Id].CrosspostSettings.DelayBeforePosting > 3)
                    _ = e.Message.DeleteOwnReactionAsync(DiscordEmoji.FromUnicode("🕒"));

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

                var task = e.Channel.CrosspostMessageAsync(msg).ContinueWith(s =>
                {
                    if (_bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostTasks.Any(x => x.MessageId == MessageId))
                    {
                        var obj = _bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostTasks.First(x => x.MessageId == MessageId);
                        _bot._guilds[e.Guild.Id].CrosspostSettings.CrosspostTasks.Remove(obj);
                    }

                    if (ReactionAdded)
                        _ = msg.DeleteOwnReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                });

                await Task.Delay(5000);

                if (!task.IsCompleted)
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(sender, 974029756355977216));
                    ReactionAdded = true;
                }
            }
        }).Add(_bot._watcher);
    }
}
