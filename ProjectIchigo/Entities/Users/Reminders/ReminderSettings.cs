namespace ProjectIchigo.Entities;

public class ReminderSettings
{
    public ReminderSettings(User user, Bot bot)
    {
        Parent = user;

        ScheduledReminders.CollectionChanged += CollectionChanged;
    }

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ScheduledReminders.Count > 10)
            ScheduledReminders.RemoveAt(0);

        foreach (var b in ScheduledReminders)
            if (!GetScheduleTasks().ContainsKey($"{Parent.UserId}; {b.UUID}; reminder"))
            {
                Task task = new(async () =>
                {
                    var user = await _bot.discordClient.Guilds.First(x => x.Value.Members.ContainsKey(Parent.UserId)).Value.GetMemberAsync(Parent.UserId);

                    await user.SendMessageAsync(new DiscordEmbedBuilder()
                        .WithDescription($"`{b.Description.Sanitize()}` - {b.DueTime.ToTimestamp(TimestampFormat.LongDateTime)} ({b.DueTime.ToTimestamp()})")
                        .WithTitle("Reminder")
                        .WithColor(EmbedColors.Info));

                    ScheduledReminders.Remove(b);
                });

                task.Add(_bot.watcher);
                task.CreateScheduleTask(b.DueTime, $"{Parent.UserId}; {b.UUID}; reminder");

                _logger.LogDebug($"Created scheduled task for reminder by '{Parent.UserId}'");
            }

        foreach (var b in GetScheduleTasks())
            if (b.Key.StartsWith($"{Parent.UserId};") && b.Key.EndsWith($"; reminder"))
            {
                var uuid = b.Key[..b.Key.LastIndexOf(";")];
                uuid = uuid[(b.Key.IndexOf(";") + 1)..];

                if (!ScheduledReminders.Any(x => x.UUID == uuid))
                {
                    DeleteScheduleTask($"{Parent.UserId}; {uuid}; reminder");

                    _logger.LogDebug($"Deleted scheduled task for reminder by '{Parent.UserId}'");
                }
            }

        _ = Bot.DatabaseClient.FullSyncDatabase();
    }



    private User Parent { get; set; }
    private Bot _bot { get; set; }


    public ObservableCollection<ReminderItem> ScheduledReminders = new();
}
