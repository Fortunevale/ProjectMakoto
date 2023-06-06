// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class ReminderSettings
{
    public ReminderSettings(User user, Bot bot)
    {
        this.Parent = user;
        this._bot = bot;

        this.ScheduledReminders.ItemsChanged += RemindersUpdated;
    }

    ~ReminderSettings()
    {
        this.ScheduledReminders.ItemsChanged -= RemindersUpdated;
    }

    private async void RemindersUpdated(object? sender, ObservableListUpdate<ReminderItem> e)
    {
        while (!this._bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        if (this.ScheduledReminders.Count > 10)
            this.ScheduledReminders.RemoveAt(0);

        foreach (var b in this.ScheduledReminders.ToList())
            if (!UniversalExtensions.GetScheduledTasks().Any(x =>
            {
                if (x.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier ||
                scheduledTaskIdentifier.Snowflake != this.Parent.UserId ||
                scheduledTaskIdentifier.Type != "reminder" ||
                scheduledTaskIdentifier.Id != b.UUID)
                    return false;

                return true;
            }))
            {
                Task task = new(async () =>
                {
                    var CommandKey = this._bot.loadedTranslations.Commands.Utility.Reminders;

                    this.ScheduledReminders.Remove(b);

                    var user = await this._bot.discordClient.Guilds.First<KeyValuePair<ulong, DiscordGuild>>(x => x.Value.Members.ContainsKey(this.Parent.UserId)).Value.GetMemberAsync(this.Parent.UserId);

                    DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"> {b.Description.FullSanitize()}\n" +
                        $"{CommandKey.CreatedOn.Get(this._bot.users[user.Id]).Build(new TVar("Guild", b.CreationPlace))}\n" +
                        $"{CommandKey.CreatedAt.Get(this._bot.users[user.Id]).Build(new TVar("Timestamp", $"{b.CreationTime.ToTimestamp()} ({b.CreationTime.ToTimestamp(TimestampFormat.LongDateTime)})"))}\n" +
                        $"{CommandKey.DueTime.Get(this._bot.users[user.Id]).Build(new TVar("Relative", b.DueTime.ToTimestamp()), new TVar("DateTime", b.DueTime.ToTimestamp(TimestampFormat.LongDateTime)))}\n" +
                        $"{(b.DueTime.GetTimespanSince() > TimeSpan.FromMinutes(2) ? $"\n\n**{CommandKey.SentLate.Get(this._bot.users[user.Id])}**" : "")}")
                        .WithTitle(CommandKey.ReminderNotification.Get(this._bot.users[user.Id]))
                        .WithColor(EmbedColors.Info));

                    var maxLength = 100 - JsonConvert.SerializeObject(new ReminderSnoozeButton(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }).Length;
                    DiscordButtonComponent snoozeButton = new(ButtonStyle.Secondary, JsonConvert.SerializeObject(new ReminderSnoozeButton
                    {
                        Description = b.Description.TruncateWithIndication(maxLength)
                    }), "Snooze", false, DiscordEmoji.FromUnicode("💤").ToComponent());
                    var msg = await user.SendMessageAsync(builder.AddComponents(snoozeButton));
                });

                task.Add(this._bot.watcher);
                task.CreateScheduledTask(b.DueTime, new ScheduledTaskIdentifier(this.Parent.UserId, b.UUID, "reminder"));

                _logger.LogDebug("Created scheduled task for reminder by '{User}'", this.Parent.UserId);
            }

        foreach (var b in UniversalExtensions.GetScheduledTasks())
        {
            if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                continue;

            if (scheduledTaskIdentifier.Snowflake == this.Parent.UserId && scheduledTaskIdentifier.Type == "reminder" && !this.ScheduledReminders.Any(x => x.UUID == ((ScheduledTaskIdentifier)b.CustomData).Id))
            {
                b.Delete();

                _logger.LogDebug("Deleted scheduled task for reminder by '{User}'", this.Parent.UserId);
            }
        }
    }

    private User Parent { get; set; }
    private Bot _bot { get; set; }

    public ObservableList<ReminderItem> ScheduledReminders = new();
}
