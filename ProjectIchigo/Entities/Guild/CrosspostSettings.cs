namespace ProjectIchigo.Entities;

internal class CrosspostSettings
{
    private int _DelayBeforePosting { get; set; } = 0;
    public int DelayBeforePosting { get => _DelayBeforePosting; set { _DelayBeforePosting = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
    
    private bool _ExcludeBots { get; set; } = false;
    public bool ExcludeBots { get => _ExcludeBots; set { _ExcludeBots = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    public ObservableCollection<ulong> CrosspostChannels { get; set; } = new();
    
    public ObservableCollection<CrosspostMessage> CrosspostTasks { get; set; } = new();

    internal NotifyCollectionChangedEventHandler CrosspostCollectionUpdated()
    {
        return (s, e) =>
        {
            _ = Bot.DatabaseClient.SyncDatabase();
        };
    }
}
