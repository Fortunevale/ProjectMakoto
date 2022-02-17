namespace Project_Ichigo.BumpReminder;
internal class BumpReminder
{
    internal BumpReminder(TaskWatcher.TaskWatcher _watcher, ServerInfo _guilds)
    {
        this._watcher = _watcher;
        this._guilds = _guilds;
    }

    TaskWatcher.TaskWatcher _watcher { get; set; }
    ServerInfo _guilds = new();

    internal void SendPersistentMessage(DiscordChannel channel, DiscordUser bUser = null)
    {
        _ = channel.SendMessageAsync(embed: new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = channel.Guild.IconUrl,
                Name = channel.Guild.Name
            },
            Color = DiscordColor.Green,
            Description = $"**The server can be bumped {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump.AddHours(2), TimestampFormat.RelativeTime)}.**\n\n" +
                                  $"The server was last bumped by <@{_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastUserId}> {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? Resources.QuestionMarkIcon : bUser.AvatarUrl)}" }
        }.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try { (await channel.GetMessageAsync(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.PersistentMessageId)).DeleteAsync().Add(_watcher); } catch { }
                _guilds.Servers[channel.Guild.Id].BumpReminderSettings.PersistentMessageId = x.Result.Id;
            }
        });
    }
}
