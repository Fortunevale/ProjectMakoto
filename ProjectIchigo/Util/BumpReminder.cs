﻿namespace ProjectIchigo;
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
            Description = $"**The server can be bumped {Formatter.Timestamp(_bot.guilds[channel.Guild.Id].BumpReminder.LastBump.AddHours(2), TimestampFormat.RelativeTime)}.**\n\n" +
                          $"The server was last bumped by <@{_bot.guilds[channel.Guild.Id].BumpReminder.LastUserId}> {Formatter.Timestamp(_bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.LongDateTime)}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? AuditLogIcons.QuestionMark : bUser.AvatarUrl)}" }
        };

        if (_bot.guilds[channel.Guild.Id].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-2))
        {
            embed.Description = $"**The server can be bumped!**\n\n" +
                          $"The server was last bumped by <@{_bot.guilds[channel.Guild.Id].BumpReminder.LastUserId}> {Formatter.Timestamp(_bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(_bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.LongDateTime)}";
            embed.Color = EmbedColors.AwaitingInput;
        }

        _ = channel.SendMessageAsync(embed.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try { (await channel.GetMessageAsync(_bot.guilds[channel.Guild.Id].BumpReminder.PersistentMessageId)).DeleteAsync().Add(_bot.watcher); } catch { }
                _bot.guilds[channel.Guild.Id].BumpReminder.PersistentMessageId = x.Result.Id;

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
            _logger.LogError($"An exception occurred while trying to un-queue previous bump messages for '{ServerId}'", ex);
        }

        var task = new Task(new Action(async () =>
        {
            _logger.LogDebug($"Executing Bump Message for '{ServerId}'");
            var Guild = await client.GetGuildAsync(ServerId);

            if (!Guild.Channels.ContainsKey(_bot.guilds[ServerId].BumpReminder.ChannelId) || _bot.guilds[ServerId].BumpReminder.BumpsMissed > 168)
            {
                _logger.LogDebug($"'{ServerId}' hasn't bumped 169 times. Disabling bump reminder..");
                _bot.guilds[ServerId].BumpReminder = new(_bot.guilds[ServerId]);
                return;
            }

            var Channel = Guild.GetChannel(_bot.guilds[ServerId].BumpReminder.ChannelId);

            _logger.LogDebug($"Checking if Self Role Message still exists, has it's reaction and is pinned in '{ServerId}'");

            try
            {
                var msg = await Channel.GetMessageAsync(_bot.guilds[ServerId].BumpReminder.MessageId);

                if (!msg.Reactions.Any(x => x.Emoji.ToString() == "✅"))
                    throw new CancelException("Self Role Message Reaction was removed.");

                if (!msg.Pinned)
                    throw new CancelException("Self Role Message is not pinned.");
            }
            catch (CancelException ex)
            {
                _bot.guilds[ServerId].BumpReminder = new(_bot.guilds[ServerId]);
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: `The bump reminder was disabled for the following reason: {ex.Message}`"));
                return;
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                _bot.guilds[ServerId].BumpReminder = new(_bot.guilds[ServerId]);
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: `The bump reminder was disabled for the following reason: Self Role Message was deleted.`"));
                return;
            }

            if (_bot.guilds[ServerId].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-3))
            {
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: <@&{_bot.guilds[ServerId].BumpReminder.RoleId}> The last bump was missed!"));
                _bot.guilds[ServerId].BumpReminder.BumpsMissed++;
            }
            else
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":bell: <@&{_bot.guilds[ServerId].BumpReminder.RoleId}> The server can be bumped again!"));

            _bot.guilds[ServerId].BumpReminder.LastReminder = DateTime.UtcNow;

            ScheduleBump(client, ServerId);
        })).CreateScheduleTask(_bot.guilds[ServerId].BumpReminder.LastReminder.AddHours(2), $"bumpmsg-{ServerId}");
    }
}
