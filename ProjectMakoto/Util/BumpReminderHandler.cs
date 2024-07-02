// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;
internal sealed class BumpReminderHandler(Bot bot) : RequiresBotReference(bot)
{
    Translations.events.bumpReminder tKey
        => this.Bot.LoadedTranslations.Events.BumpReminder;

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
            Description = $"**{this.tKey.NextBumpTime.Get(this.Bot.Guilds[channel.Guild.Id]).Build(new TVar("Timestamp", this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump.ToTimestamp()))}**\n\n" +
                          $"{this.tKey.LastBumpBy.Get(this.Bot.Guilds[channel.Guild.Id]).Build(new TVar("User", $"<@{this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastUserId}>"), new TVar("RTimestamp", this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump.ToTimestamp()), new TVar("FTimestamp", this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump.ToTimestamp(TimestampFormat.LongDateTime)))}",
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = $"{(bUser is null ? AuditLogIcons.QuestionMark : bUser.AvatarUrl)}" }
        };

        if (this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-2))
        {
            embed.Description = $"**{this.tKey.ServerCanBeBump.Get(this.Bot.Guilds[channel.Guild.Id])}**\n\n" +
                          $"{this.tKey.LastBumpBy.Get(this.Bot.Guilds[channel.Guild.Id]).Build(new TVar("User", $"<@{this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastUserId}>"), new TVar("RTimestamp", this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump.ToTimestamp()), new TVar("FTimestamp", this.Bot.Guilds[channel.Guild.Id].BumpReminder.LastBump.ToTimestamp(TimestampFormat.LongDateTime)))}";
            embed.Color = EmbedColors.AwaitingInput;
        }

        _ = channel.SendMessageAsync(embed.Build()).ContinueWith(async x =>
        {
            if (x.IsCompletedSuccessfully)
            {
                try
                { _ = (await channel.GetMessageAsync(this.Bot.Guilds[channel.Guild.Id].BumpReminder.PersistentMessageId)).DeleteAsync().Add(this.Bot); }
                catch { }
                this.Bot.Guilds[channel.Guild.Id].BumpReminder.PersistentMessageId = x.Result.Id;

                _ = channel.DeleteMessagesAsync((await channel.GetMessagesAsync(100)).Where(y => y.Embeds.Any() && y.Author.Id == client.CurrentUser.Id && y.Id != x.Result.Id));
            }
        });
    }

    internal void ScheduleBump(DiscordClient client, ulong ServerId)
    {
        Log.Debug("Queuing Bump Message for '{Guild}'", ServerId);

        try
        {
            foreach (var b in ScheduledTaskExtensions.GetScheduledTasks())
            {
                if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                    continue;

                if (scheduledTaskIdentifier.Snowflake == ServerId && scheduledTaskIdentifier.Type == "bumpmsg")
                    b.Delete();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while trying to un-queue previous bump messages for '{Guild}'", ServerId);
        }

        _ = new Func<Task>(async () =>
        {
            Log.Debug("Executing Bump Message for '{Guild}'", ServerId);
            var Guild = await client.GetGuildAsync(ServerId);

            if (!Guild.Channels.ContainsKey(this.Bot.Guilds[ServerId].BumpReminder.ChannelId) || this.Bot.Guilds[ServerId].BumpReminder.BumpsMissed > 168)
            {
                Log.Debug("'{Guild}' has deleted their bump channel or hasn't bumped 169 times. Disabling bump reminder..", ServerId);
                this.Bot.Guilds[ServerId].BumpReminder.Reset();
                return;
            }

            var Channel = Guild.GetChannel(this.Bot.Guilds[ServerId].BumpReminder.ChannelId);

            Log.Debug("Checking if Self Role Message still exists, has it's reaction and is pinned in '{Guild}'", ServerId);

            try
            {
                if (!Channel.TryGetMessage(this.Bot.Guilds[ServerId].BumpReminder.MessageId, out var msg))
                    throw new CancelException(this.tKey.BumpReminderDisabledMessageDeleted.Get(this.Bot.Guilds[ServerId]));

                if (!msg.Reactions.Any(x => x.Emoji.ToString() == "✅"))
                    throw new CancelException(this.tKey.BumpReminderDisabledReactionRemoved.Get(this.Bot.Guilds[ServerId]));

                if (!msg.Pinned)
                    throw new CancelException(this.tKey.BumpReminderDisabledNotPinned.Get(this.Bot.Guilds[ServerId]));
            }
            catch (CancelException ex)
            {
                this.Bot.Guilds[ServerId].BumpReminder.Reset();
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: `{this.tKey.BumpReminderDisabled.Get(this.Bot.Guilds[ServerId])} {ex.Message}`"));
                return;
            }

            if (this.Bot.Guilds[ServerId].BumpReminder.LastBump < DateTime.UtcNow.AddHours(-3))
            {
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":warning: <@&{this.Bot.Guilds[ServerId].BumpReminder.RoleId}> {this.tKey.LastBumpMissed.Get(this.Bot.Guilds[ServerId])}"));
                this.Bot.Guilds[ServerId].BumpReminder.BumpsMissed++;
            }
            else
                _ = Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($":bell: <@&{this.Bot.Guilds[ServerId].BumpReminder.RoleId}> {this.tKey.BumpNotification.Get(this.Bot.Guilds[ServerId])}"));

            this.Bot.Guilds[ServerId].BumpReminder.LastReminder = DateTime.UtcNow;

            this.ScheduleBump(client, ServerId);
        }).CreateScheduledTask(this.Bot.Guilds[ServerId].BumpReminder.LastReminder.AddHours(2), new ScheduledTaskIdentifier(ServerId, "", "bumpmsg"));
    }
}
