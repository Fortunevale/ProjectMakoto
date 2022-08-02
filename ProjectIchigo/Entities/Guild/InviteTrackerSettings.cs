namespace ProjectIchigo.Entities;

public class InviteTrackerSettings
{
    public InviteTrackerSettings(Guild guild)
    {
        Parent = guild;
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

    private List<InviteTrackerCacheItem> _Cache { get; set; } = new();
    public List<InviteTrackerCacheItem> Cache 
    {
        get => _Cache;
        set
        {
            _Cache = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "invitetracker_cache", JsonConvert.SerializeObject(value), Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
