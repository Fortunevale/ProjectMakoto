// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class ReminderSettings
{
    public ReminderSettings(User user, Bot bot)
    {
        Parent = user;
        _bot = bot;

        ScheduledReminders.ItemsChanged += RemindersUpdated;
    }

    ~ReminderSettings()
    {
        ScheduledReminders.ItemsChanged -= RemindersUpdated;
    }

    private async void RemindersUpdated(object? sender, ObservableListUpdate<ReminderItem> e)
    {
        while (!_bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        if (ScheduledReminders.Count > 10)
            ScheduledReminders.RemoveAt(0);

        foreach (var b in ScheduledReminders.ToList())
            if (!GetScheduleTasks().ToList().Any(x => x.Value.customId == $"{Parent.UserId}; {b.UUID}; reminder"))
            {
                Task task = new(async () =>
                {
                    this.ScheduledReminders.Remove(b);

                    var user = await this._bot.discordClient.Guilds.First<KeyValuePair<ulong, DiscordGuild>>(x => x.Value.Members.ContainsKey(this.Parent.UserId)).Value.GetMemberAsync(this.Parent.UserId);

                    DiscordMessageBuilder builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"> {b.Description.FullSanitize()}\n" +
                        $"Created on {b.CreationPlace}\n" +
                        $"Created at {b.CreationTime.ToTimestamp()} ({b.CreationTime.ToTimestamp(TimestampFormat.LongDateTime)})\n" +
                        $"Due {b.DueTime.ToTimestamp()} ({b.DueTime.ToTimestamp(TimestampFormat.LongDateTime)})" +
                        $"{(b.DueTime.GetTimespanSince() > TimeSpan.FromMinutes(2) ? "\n\n**This reminder has been sent late because of a recent bot outage.**" : "")}")
                        .WithTitle("Reminder Notification")
                        .WithColor(EmbedColors.Info));

                    var maxLength = 100 - JsonConvert.SerializeObject(new ReminderSnoozeButton(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }).Length;
                    DiscordButtonComponent snoozeButton = new(ButtonStyle.Secondary, JsonConvert.SerializeObject(new ReminderSnoozeButton
                    {
                        Description = b.Description.TruncateWithIndication(maxLength)
                    }), "Snooze", false, DiscordEmoji.FromUnicode("💤").ToComponent());
                    var msg = await user.SendMessageAsync(builder.AddComponents(snoozeButton));
                });

                task.Add(_bot.watcher);
                task.CreateScheduleTask(b.DueTime, $"{Parent.UserId}; {b.UUID}; reminder");

                _logger.LogDebug("Created scheduled task for reminder by '{User}'", Parent.UserId);
            }

        foreach (var b in GetScheduleTasks().ToList())
            if (b.Value.customId.StartsWith($"{Parent.UserId};") && b.Value.customId.EndsWith($"reminder"))
            {
                var uuid = b.Value.customId[..b.Value.customId.LastIndexOf(";")];
                uuid = uuid[(uuid.IndexOf(";") + 2)..];

                if (!ScheduledReminders.Any(x => x.UUID == uuid))
                {
                    DeleteScheduleTask(b.Key);

                    _logger.LogDebug("Deleted scheduled task for reminder by '{User}'", Parent.UserId);
                }
            }
    }

    private User Parent { get; set; }
    private Bot _bot { get; set; }

    public ObservableList<ReminderItem> ScheduledReminders = new();
}
