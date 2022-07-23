namespace ProjectIchigo.Entities;

public class InviteTrackerSettings
{
    public InviteTrackerSettings(Guild guild)
    {
        Cache.CollectionChanged += Cache_CollectionChanged;

        Parent = guild;
    }

    ~InviteTrackerSettings()
    {
        Cache.CollectionChanged -= Cache_CollectionChanged;
    }

    private void Cache_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _ = Bot.DatabaseClient.SyncDatabase();
    }

    private Guild Parent { get; set; }


    private bool _Enabled { get; set; } = false;
    public bool Enabled 
    { 
        get => _Enabled; 
        set 
        { 
            _Enabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "invitetracker_enabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    public ObservableCollection<InviteTrackerCacheItem> Cache { get; set; } = new();
}
