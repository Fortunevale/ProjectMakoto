namespace Project_Ichigo.Events;
internal class AfkEvents
{
    TaskWatcher.TaskWatcher _watcher { get; set; }
    Users _users { set; get; }

    internal AfkEvents(TaskWatcher.TaskWatcher _watcher, Users _users)
    {
        this._watcher = _watcher;
        this._users = _users;
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate || e.Message.Content.StartsWith(">>") || e.Message.Content.StartsWith(";;") || e.Author.IsBot)
                return;

            if (!_users.List.ContainsKey(e.Author.Id))
                _users.List.Add(e.Author.Id, new Users.Info());

            if (_users.List[e.Author.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch && _users.List[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
            {
                DateTime cache = new DateTime().ToUniversalTime().AddTicks(_users.List[e.Author.Id].AfkStatus.TimeStamp.Ticks);

                _users.List[e.Author.Id].AfkStatus.Reason = "";
                _users.List[e.Author.Id].AfkStatus.TimeStamp = DateTime.UnixEpoch;

                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Timestamp = DateTime.UtcNow,
                    Description = $"{e.Author.Mention} `You're no longer afk. You've been afk for {cache.GetTotalSecondsSince().GetHumanReadable()}.`"
                };

                bool ExtendDelay = false;

                if (_users.List[e.Author.Id].AfkStatus.MessagesAmount > 0)
                {
                    embed.Description += $"\n\n`Here is what you missed:`\n" +
                                         $"{string.Join("\n", _users.List[e.Author.Id].AfkStatus.Messages.Select(x => $"[`Message`](https://discord.com/channels/{x.GuildId}/{x.ChannelId}/{x.MessageId}) by <@!{x.AuthorId}>"))}";

                    ExtendDelay = true;

                    if (_users.List[e.Author.Id].AfkStatus.MessagesAmount - 5 > 0)
                    {
                        embed.Description += $"\n`And {_users.List[e.Author.Id].AfkStatus.MessagesAmount - 5} more..`";
                    }

                    _users.List[e.Author.Id].AfkStatus.MessagesAmount = 0;
                    _users.List[e.Author.Id].AfkStatus.Messages = new();
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

                    if (!_users.List.ContainsKey(b.Id))
                        _users.List.Add(b.Id, new Users.Info());

                    if (_users.List[b.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch)
                    {
                        if (_users.List[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(30) > DateTime.UtcNow)
                            return;

                        if (_users.List[b.Id].AfkStatus.Messages.Count < 5)
                        {
                            _users.List[b.Id].AfkStatus.Messages.Add(new AfkStatusMessageCache
                            {
                                AuthorId = e.Author.Id,
                                ChannelId = e.Channel.Id,
                                GuildId = e.Guild.Id,
                                MessageId = e.Message.Id,
                            });
                        }

                        _users.List[b.Id].AfkStatus.MessagesAmount++;

                        _users.List[e.Author.Id].AfkStatus.LastMentionTrigger = DateTime.UtcNow;

                        _ = e.Message.RespondAsync(new DiscordEmbedBuilder
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                            Color = ColorHelper.Info,
                            Timestamp = DateTime.UtcNow,
                            Description = $"{b.Mention} `is currently AFK and has been since` {Formatter.Timestamp(_users.List[b.Id].AfkStatus.TimeStamp)}`, they most likely wont answer your message: '{_users.List[b.Id].AfkStatus.Reason}'`"
                        }).ContinueWith(async x =>
                        {
                            await Task.Delay(10000);
                            _ = x.Result.DeleteAsync();
                        });
                        return;
                    }
                }
            }

        }).Add(_watcher);
    }
}
