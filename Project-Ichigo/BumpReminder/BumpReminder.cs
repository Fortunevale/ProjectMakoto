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

    internal void SendPersistentMessage(DiscordClient client, DiscordChannel channel, DiscordUser bUser = null)
    {
        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = channel.Guild.IconUrl,
                Name = channel.Guild.Name
            },
            Color = ColorHelper.Info,
            Description = $"**The server can be bumped {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump.AddHours(2), TimestampFormat.RelativeTime)}.**\n\n" +
                          $"The server was last bumped by <@{_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastUserId}> {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? Resources.QuestionMarkIcon : bUser.AvatarUrl)}" }
        };

        if (_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump < DateTime.UtcNow.AddHours(-2))
        {
            embed.Description = $"**The server can be bumped!**\n\n" +
                          $"The server was last bumped by <@{_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastUserId}> {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}";
            embed.Color = ColorHelper.Error;
        }

        _ = channel.SendMessageAsync(embed.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try { (await channel.GetMessageAsync(_guilds.Servers[channel.Guild.Id].BumpReminderSettings.PersistentMessageId)).DeleteAsync().Add(_watcher); } catch { }
                _guilds.Servers[channel.Guild.Id].BumpReminderSettings.PersistentMessageId = x.Result.Id;

                _ = channel.DeleteMessagesAsync((await channel.GetMessagesAsync(100)).Where(y => y.Embeds.Any() && y.Author.Id == client.CurrentUser.Id && y.Id != x.Result.Id));
            }
        });
    }

    internal void ScheduleBump(DiscordClient client, ulong ServerId)
    {
        if (GetScheduleTasks() != null)
            if (GetScheduleTasks().Any(x => x.Value.customId == $"bumpmsg-{ServerId}"))
                DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ServerId}").Key);

        var task = new Task(new Action(async () =>
        {
            var Guild = await client.GetGuildAsync(ServerId);

            if (!Guild.Channels.ContainsKey(_guilds.Servers[ServerId].BumpReminderSettings.ChannelId))
            {
                _guilds.Servers[ServerId].BumpReminderSettings = new();
                return;
            }

            var Channel = Guild.GetChannel(_guilds.Servers[ServerId].BumpReminderSettings.ChannelId);

            if (_guilds.Servers[ServerId].BumpReminderSettings.LastBump < DateTime.UtcNow.AddHours(-3))
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: <@&{_guilds.Servers[ServerId].BumpReminderSettings.RoleId}> The last bump was missed!"));
            else
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":bell: <@&{_guilds.Servers[ServerId].BumpReminderSettings.RoleId}> The server can be bumped again!"));

            _guilds.Servers[ServerId].BumpReminderSettings.LastReminder = DateTime.UtcNow;

            ScheduleBump(client, ServerId);
        })).CreateScheduleTask(_guilds.Servers[ServerId].BumpReminderSettings.LastReminder.AddHours(2), $"bumpmsg-{ServerId}");
    }
}
