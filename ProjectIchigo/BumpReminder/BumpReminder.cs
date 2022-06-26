namespace ProjectIchigo;
internal class BumpReminder
{
    internal BumpReminder(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal void SendPersistentMessage(DiscordClient client, DiscordChannel channel, DiscordUser bUser = null)
    {
        var embed = new DiscordEmbedBuilder
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor
            {
                IconUrl = channel.Guild.IconUrl,
                Name = channel.Guild.Name
            },
            Color = EmbedColors.Info,
            Description = $"**The server can be bumped {Formatter.Timestamp(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump.AddHours(2), TimestampFormat.RelativeTime)}.**\n\n" +
                          $"The server was last bumped by <@{_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastUserId}> {Formatter.Timestamp(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? Resources.QuestionMarkIcon : bUser.AvatarUrl)}" }
        };

        if (_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump < DateTime.UtcNow.AddHours(-2))
        {
            embed.Description = $"**The server can be bumped!**\n\n" +
                          $"The server was last bumped by <@{_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastUserId}> {Formatter.Timestamp(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.LastBump, TimestampFormat.LongDateTime)}";
            embed.Color = EmbedColors.AwaitingInput;
        }

        _ = channel.SendMessageAsync(embed.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try { (await channel.GetMessageAsync(_bot._guilds.List[channel.Guild.Id].BumpReminderSettings.PersistentMessageId)).DeleteAsync().Add(_bot._watcher); } catch { }
                _bot._guilds.List[channel.Guild.Id].BumpReminderSettings.PersistentMessageId = x.Result.Id;

                _ = channel.DeleteMessagesAsync((await channel.GetMessagesAsync(100)).Where(y => y.Embeds.Any() && y.Author.Id == client.CurrentUser.Id && y.Id != x.Result.Id));
            }
        });
    }

    internal void ScheduleBump(DiscordClient client, ulong ServerId)
    {
        _logger.LogDebug($"Queuing Bump Message for '{ServerId}'");

        try
        {
            if (GetScheduleTasks() is not null)
                if (GetScheduleTasks()?.Any(x => x.Value.customId == $"bumpmsg-{ServerId}") ?? false)
                    DeleteScheduleTask(GetScheduleTasks().First(x => x.Value.customId == $"bumpmsg-{ServerId}").Key);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception occured while trying to un-queue previous bump messages for '{ServerId}'", ex);
        }

        var task = new Task(new Action(async () =>
        {
            _logger.LogDebug($"Executing Bump Message for '{ServerId}'");
            var Guild = await client.GetGuildAsync(ServerId);

            if (!Guild.Channels.ContainsKey(_bot._guilds.List[ServerId].BumpReminderSettings.ChannelId) || _bot._guilds.List[ServerId].BumpReminderSettings.BumpsMissed > 168)
            {
                _bot._guilds.List[ServerId].BumpReminderSettings = new();
                return;
            }

            var Channel = Guild.GetChannel(_bot._guilds.List[ServerId].BumpReminderSettings.ChannelId);

            if (_bot._guilds.List[ServerId].BumpReminderSettings.LastBump < DateTime.UtcNow.AddHours(-3))
            {
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: <@&{_bot._guilds.List[ServerId].BumpReminderSettings.RoleId}> The last bump was missed!"));
                _bot._guilds.List[ServerId].BumpReminderSettings.BumpsMissed++;
            }
            else
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":bell: <@&{_bot._guilds.List[ServerId].BumpReminderSettings.RoleId}> The server can be bumped again!"));

            _bot._guilds.List[ServerId].BumpReminderSettings.LastReminder = DateTime.UtcNow;

            ScheduleBump(client, ServerId);
        })).CreateScheduleTask(_bot._guilds.List[ServerId].BumpReminderSettings.LastReminder.AddHours(2), $"bumpmsg-{ServerId}");
    }
}
