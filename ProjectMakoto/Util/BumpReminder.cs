// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;
internal sealed class BumpReminder
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
            Description = $"**The server can be bumped {Formatter.Timestamp(this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump.AddHours(2), TimestampFormat.RelativeTime)}.**\n\n" +
                          $"The server was last bumped by <@{this._bot.guilds[channel.Guild.Id].BumpReminder.LastUserId}> {Formatter.Timestamp(this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.LongDateTime)}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? AuditLogIcons.QuestionMark : bUser.AvatarUrl)}" }
        };

        if (this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-2))
        {
            embed.Description = $"**The server can be bumped!**\n\n" +
                          $"The server was last bumped by <@{this._bot.guilds[channel.Guild.Id].BumpReminder.LastUserId}> {Formatter.Timestamp(this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.RelativeTime)} at {Formatter.Timestamp(this._bot.guilds[channel.Guild.Id].BumpReminder.LastBump, TimestampFormat.LongDateTime)}";
            embed.Color = EmbedColors.AwaitingInput;
        }

        _ = channel.SendMessageAsync(embed.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try
                { (await channel.GetMessageAsync(this._bot.guilds[channel.Guild.Id].BumpReminder.PersistentMessageId)).DeleteAsync().Add(this._bot.watcher); }
                catch { }
                this._bot.guilds[channel.Guild.Id].BumpReminder.PersistentMessageId = x.Result.Id;

                _ = channel.DeleteMessagesAsync((await channel.GetMessagesAsync(100)).Where(y => y.Embeds.Any() && y.Author.Id == client.CurrentUser.Id && y.Id != x.Result.Id));
            }
        });
    }

    internal void ScheduleBump(DiscordClient client, ulong ServerId)
    {
        _logger.LogDebug("Queuing Bump Message for '{Guild}'", ServerId);

        try
        {
            foreach (var b in UniversalExtensions.GetScheduledTasks())
            {
                if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                    continue;

                if (scheduledTaskIdentifier.Snowflake == ServerId && scheduledTaskIdentifier.Type == "bumpmsg")
                    b.Delete();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred while trying to un-queue previous bump messages for '{Guild}'", ex, ServerId);
        }

        var task = new Task(new Action(async () =>
        {
            _logger.LogDebug("Executing Bump Message for '{Guild}'", ServerId);
            var Guild = await client.GetGuildAsync(ServerId);

            if (!Guild.Channels.ContainsKey(this._bot.guilds[ServerId].BumpReminder.ChannelId) || this._bot.guilds[ServerId].BumpReminder.BumpsMissed > 168)
            {
                _logger.LogDebug("'{Guild}' hasn't bumped 169 times. Disabling bump reminder..", ServerId);
                this._bot.guilds[ServerId].BumpReminder = new(this._bot.guilds[ServerId]);
                return;
            }

            var Channel = Guild.GetChannel(this._bot.guilds[ServerId].BumpReminder.ChannelId);

            _logger.LogDebug("Checking if Self Role Message still exists, has it's reaction and is pinned in '{Guild}'", ServerId);

            try
            {
                var msg = await Channel.GetMessageAsync(this._bot.guilds[ServerId].BumpReminder.MessageId);

                if (!msg.Reactions.Any(x => x.Emoji.ToString() == "✅"))
                    throw new CancelException("Self Role Message Reaction was removed.");

                if (!msg.Pinned)
                    throw new CancelException("Self Role Message is not pinned.");
            }
            catch (CancelException ex)
            {
                this._bot.guilds[ServerId].BumpReminder = new(this._bot.guilds[ServerId]);
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: `The bump reminder was disabled for the following reason: {ex.Message}`"));
                return;
            }
            catch (DisCatSharp.Exceptions.NotFoundException)
            {
                this._bot.guilds[ServerId].BumpReminder = new(this._bot.guilds[ServerId]);
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: `The bump reminder was disabled for the following reason: Self Role Message was deleted.`"));
                return;
            }

            if (this._bot.guilds[ServerId].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-3))
            {
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: <@&{this._bot.guilds[ServerId].BumpReminder.RoleId}> The last bump was missed!"));
                this._bot.guilds[ServerId].BumpReminder.BumpsMissed++;
            }
            else
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":bell: <@&{this._bot.guilds[ServerId].BumpReminder.RoleId}> The server can be bumped again!"));

            this._bot.guilds[ServerId].BumpReminder.LastReminder = DateTime.UtcNow;

            ScheduleBump(client, ServerId);
        })).CreateScheduledTask(this._bot.guilds[ServerId].BumpReminder.LastReminder.AddHours(2), new ScheduledTaskIdentifier(ServerId, "", "bumpmsg"));
    }
}
