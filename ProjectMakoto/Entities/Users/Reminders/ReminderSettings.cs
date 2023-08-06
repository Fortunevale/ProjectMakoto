// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class ReminderSettings : RequiresParent<User>
{
    public ReminderSettings(Bot bot, User parent) : base(bot, parent)
    {
        this._ScheduledReminders.ItemsChanged += this.RemindersUpdated;
    }

    ~ReminderSettings()
    {
        this.ScheduledReminders.ItemsChanged -= this.RemindersUpdated;
    }

    public ObservableList<ReminderItem> ScheduledReminders { get => this._ScheduledReminders; set { this._ScheduledReminders = value; this._ScheduledReminders.ItemsChanged += this.RemindersUpdated; } }
    private ObservableList<ReminderItem> _ScheduledReminders { get; set; } = new();

    private async void RemindersUpdated(object? sender, ObservableListUpdate<ReminderItem> e)
    {
        while (!this.Bot.status.DiscordGuildDownloadCompleted)
            await Task.Delay(1000);

        if (this.ScheduledReminders.Count > 10)
            this.ScheduledReminders.RemoveAt(0);

        foreach (var b in this.ScheduledReminders.ToList())
            if (!ScheduledTaskExtensions.GetScheduledTasks().ContainsTask("reminder", this.Parent.Id, b.UUID))
            {
                Task task = new(async () =>
                {
                    var CommandKey = this.Bot.LoadedTranslations.Commands.Utility.Reminders;

                    _ = this.ScheduledReminders.Remove(b);

                    var user = await this.Bot.DiscordClient.Guilds.First<KeyValuePair<ulong, DiscordGuild>>(x => x.Value.Members.ContainsKey(this.Parent.Id)).Value.GetMemberAsync(this.Parent.Id);

                    var builder = new DiscordMessageBuilder().WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"> {b.Description.FullSanitize()}\n" +
                        $"{CommandKey.CreatedOn.Get(this.Bot.Users[user.Id]).Build(new TVar("Guild", b.CreationPlace))}\n" +
                        $"{CommandKey.CreatedAt.Get(this.Bot.Users[user.Id]).Build(new TVar("Timestamp", $"{b.CreationTime.ToTimestamp()} ({b.CreationTime.ToTimestamp(TimestampFormat.LongDateTime)})"))}\n" +
                        $"{CommandKey.DueTime.Get(this.Bot.Users[user.Id]).Build(new TVar("Relative", b.DueTime.ToTimestamp()), new TVar("DateTime", b.DueTime.ToTimestamp(TimestampFormat.LongDateTime)))}\n" +
                        $"{(b.DueTime.GetTimespanSince() > TimeSpan.FromMinutes(2) ? $"\n\n**{CommandKey.SentLate.Get(this.Bot.Users[user.Id])}**" : "")}")
                        .WithTitle(CommandKey.ReminderNotification.Get(this.Bot.Users[user.Id]))
                        .WithColor(EmbedColors.Info));

                    var maxLength = 100 - JsonConvert.SerializeObject(new ReminderSnoozeButton(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include }).Length;
                    DiscordButtonComponent snoozeButton = new(ButtonStyle.Secondary, JsonConvert.SerializeObject(new ReminderSnoozeButton
                    {
                        Description = b.Description.TruncateWithIndication(maxLength)
                    }), CommandKey.Snooze.Get(this.Bot.Users[user.Id]), false, DiscordEmoji.FromUnicode("💤").ToComponent());
                    var msg = await user.SendMessageAsync(builder.AddComponents(snoozeButton));
                });

                _ = task.Add(this.Bot);
                _ = task.CreateScheduledTask(b.DueTime, new ScheduledTaskIdentifier(this.Parent.Id, b.UUID, "reminder"));

                _logger.LogDebug("Created scheduled task for reminder by '{User}'", this.Parent.Id);
            }

        foreach (var b in ScheduledTaskExtensions.GetScheduledTasks())
        {
            if (b.CustomData is not ScheduledTaskIdentifier scheduledTaskIdentifier)
                continue;

            if (scheduledTaskIdentifier.Snowflake == this.Parent.Id && scheduledTaskIdentifier.Type == "reminder" && !this.ScheduledReminders.Any(x => x.UUID == ((ScheduledTaskIdentifier)b.CustomData).Id))
            {
                b.Delete();

                _logger.LogDebug("Deleted scheduled task for reminder by '{User}'", this.Parent.Id);
            }
        }
    }
}
