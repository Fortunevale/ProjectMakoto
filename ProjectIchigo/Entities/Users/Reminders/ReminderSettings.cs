namespace ProjectIchigo.Entities;

public class ReminderSettings
{
    public ReminderSettings(User user)
    {
        Parent = user;

        ScheduledReminders.CollectionChanged += CollectionChanged;
    }

    ~ReminderSettings()
        => ScheduledReminders.CollectionChanged -= CollectionChanged;

    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ScheduledReminders.Count > 10)
            ScheduledReminders.RemoveAt(0);

        _ = Bot.DatabaseClient.FullSyncDatabase();
    }



    private User Parent { get; set; }


    public ObservableCollection<ReminderItem> ScheduledReminders = new();
}
