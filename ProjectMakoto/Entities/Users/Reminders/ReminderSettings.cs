namespace ProjectMakoto.Entities;

public class ReminderSettings
{
    public ReminderSettings(User user, Bot bot)
    {
        Parent = user;
        _bot = bot;

        ScheduledReminders.ItemsChanged += RemindersUpdated;
        OldReminders.ItemsChanged += OldReminders_ItemsChanged;
    }

    ~ReminderSettings()
    {
        ScheduledReminders.ItemsChanged -= RemindersUpdated;
        OldReminders.ItemsChanged -= OldReminders_ItemsChanged;
    }

    private void OldReminders_ItemsChanged(object? sender, ObservableListUpdate<ReminderItem> e)
    {
        if (OldReminders.Count > 10)
            OldReminders.RemoveAt(0);
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
                    this.OldReminders.Add(b);
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

                    DiscordButtonComponent snoozeButton = new(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Snooze", false, DiscordEmoji.FromUnicode("💤").ToComponent());
                    var msg = await user.SendMessageAsync(builder.AddComponents(snoozeButton));

                    var button = await msg.WaitForButtonAsync(TimeSpan.FromMinutes(30));

                    if (button.TimedOut)
                    {
                        _ = msg.ModifyAsync(builder);
                        return;
                    }

                    _ = msg.DeleteAsync();

                    await button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    var newMsg = await button.Result.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder().WithDescription("`How long do you want to snooze for?`").AsBotAwaitingInput(_bot.discordClient, user, _bot.users[user.Id]))
                        .AddComponents(new DiscordStringSelectComponent("Select a new due time..", new List<DiscordStringSelectComponentOption>
                        {
                            new DiscordStringSelectComponentOption("1 minute", "1m"),
                            new DiscordStringSelectComponentOption("3 minutes", "3m"),
                            new DiscordStringSelectComponentOption("5 minutes", "5m"),
                            new DiscordStringSelectComponentOption("10 minutes", "10m"),
                            new DiscordStringSelectComponentOption("20 minutes", "20m"),
                            new DiscordStringSelectComponentOption("30 minutes", "30m"),
                            new DiscordStringSelectComponentOption("1 hour", "1h"),
                            new DiscordStringSelectComponentOption("2 hours", "2h"),
                            new DiscordStringSelectComponentOption("6 hours", "6h"),
                            new DiscordStringSelectComponentOption("12 hours", "12h"),
                            new DiscordStringSelectComponentOption("1 day", "1d"),
                            new DiscordStringSelectComponentOption("3 days", "3d"),
                            new DiscordStringSelectComponentOption("7 days", "7d"),
                            new DiscordStringSelectComponentOption("14 days", "14d"),
                        })));

                    var button2 = await newMsg.WaitForSelectAsync(x => x.User.Id == button.Result.User.Id, ComponentType.StringSelect, TimeSpan.FromMinutes(5));

                    if (button.TimedOut)
                    {
                        _ = newMsg.DeleteAsync();
                        return;
                    }

                    _ = button2.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    _ = button2.Result.Message.DeleteAsync();

                    b.UUID = Guid.NewGuid().ToString();
                    b.CreationTime = DateTime.UtcNow;
                    b.CreationPlace = $"[`Direct Messages`](https://discord.com/channels/@me/{msg.Channel.Id})";
                    b.DueTime = button2.Result.Values[0] switch
                    {
                        "1m" => DateTime.UtcNow.AddMinutes(1),
                        "3m" => DateTime.UtcNow.AddMinutes(3),
                        "5m" => DateTime.UtcNow.AddMinutes(5),
                        "10m" => DateTime.UtcNow.AddMinutes(10),
                        "20m" => DateTime.UtcNow.AddMinutes(20),
                        "30m" => DateTime.UtcNow.AddMinutes(30),
                        "1h" => DateTime.UtcNow.AddHours(1),
                        "2h" => DateTime.UtcNow.AddHours(2),
                        "6h" => DateTime.UtcNow.AddHours(6),
                        "12h" => DateTime.UtcNow.AddHours(12),
                        "1d" => DateTime.UtcNow.AddDays(1),
                        "3d" => DateTime.UtcNow.AddDays(3),
                        "7d" => DateTime.UtcNow.AddDays(7),
                        "14d" => DateTime.UtcNow.AddDays(14),
                        _ => DateTime.UtcNow.AddMinutes(1),
                    };

                    this.OldReminders.Remove(b);
                    this.ScheduledReminders.Add(b);
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
    public ObservableList<ReminderItem> OldReminders = new();
}
