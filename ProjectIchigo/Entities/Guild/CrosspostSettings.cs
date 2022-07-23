namespace ProjectIchigo.Entities;

public class CrosspostSettings
{
    public CrosspostSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private int _DelayBeforePosting { get; set; } = 0;
    public int DelayBeforePosting 
    { 
        get => _DelayBeforePosting; 
        set 
        { 
            _DelayBeforePosting = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "crosspostdelay", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
    
    private bool _ExcludeBots { get; set; } = false;
    public bool ExcludeBots 
    { 
        get => _ExcludeBots; 
        set 
        { 
            _ExcludeBots = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "crosspostexcludebots", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

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
