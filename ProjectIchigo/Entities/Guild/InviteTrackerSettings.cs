namespace ProjectIchigo.Entities;

internal class InviteTrackerSettings
{
    public InviteTrackerSettings()
    {
        Cache.CollectionChanged += Cache_CollectionChanged;
    }

    ~InviteTrackerSettings()
    {
        Cache.CollectionChanged -= Cache_CollectionChanged;
    }

    private void Cache_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _ = Bot.DatabaseClient.SyncDatabase();
    }

    private bool _Enabled { get; set; } = false;
    public bool Enabled { get => _Enabled; set { _Enabled = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    public ObservableCollection<InviteTrackerCacheItem> Cache { get; set; } = new();
}
