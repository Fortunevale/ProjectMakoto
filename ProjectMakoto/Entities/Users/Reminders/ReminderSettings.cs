// Project Makoto
// Copyright (C) 2024  Fortunevale
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
        this.RemindersUpdated();
    }

    [ColumnName("reminders"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public ReminderItem[] ScheduledReminders
    {
        get => JsonConvert.DeserializeObject<ReminderItem[]>(this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "reminders", this.Bot.DatabaseClient.mainDatabaseConnection));
        set
        {
            _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "reminders", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
            this.RemindersUpdated();
        }
    }

    private void RemindersUpdated()
    {
        _ = Task.Run(async () =>
        {
            while (!this.Bot.status.DiscordGuildDownloadCompleted)
                await Task.Delay(1000);

            if (this.ScheduledReminders.Length > 10)
                this.ScheduledReminders = this.ScheduledReminders.Take(10).ToArray();

            foreach (var b in this.ScheduledReminders.ToList())
                if (!ScheduledTaskExtensions.GetScheduledTasks().ContainsTask("reminder", this.Parent.Id, b.UUID))
                {
                    Func<Task> task = new(async () =>
                    {
                        var CommandKey = this.Bot.LoadedTranslations.Commands.Utility.Reminders;

                        this.ScheduledReminders = this.ScheduledReminders.Remove(x => x.ToString(), b);

                        var user = await this.Bot.DiscordClient.GetFirstShard().GetUserAsync(this.Parent.Id);

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
        });
    }
}
