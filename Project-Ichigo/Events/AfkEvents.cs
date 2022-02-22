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
            if (e.Guild == null || e.Channel.IsPrivate)
                return;

            if (!_users.List.ContainsKey(e.Author.Id))
                _users.List.Add(e.Author.Id, new Users.Info());

            if (_users.List[e.Author.Id].AfkStatus.TimeStamp != DateTime.UnixEpoch && _users.List[e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
            {
                _users.List[e.Author.Id].AfkStatus.Reason = "";
                _users.List[e.Author.Id].AfkStatus.TimeStamp = DateTime.UnixEpoch;

                _ = e.Message.RespondAsync(new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                    Color = ColorHelper.Info,
                    Timestamp = DateTime.UtcNow,
                    Description = $"{e.Author.Mention} `You're no longer afk.`"
                }).ContinueWith(async x =>
                {
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

                        _users.List[e.Author.Id].AfkStatus.LastMentionTrigger = DateTime.UtcNow;

                        _ = e.Message.RespondAsync(new DiscordEmbedBuilder
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"Afk Status • {e.Guild.Name}" },
                            Color = ColorHelper.Info,
                            Timestamp = DateTime.UtcNow,
                            Description = $"{b.Mention} `is currently AFK and has been since`{Formatter.Timestamp(_users.List[b.Id].AfkStatus.TimeStamp)}`, they most likely wont answer your message: '{_users.List[b.Id].AfkStatus.Reason}'`"
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
