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

    public Dictionary<ulong, CrosspostRatelimit> CrosspostRatelimits { get; set; } = new();

    public ObservableCollection<CrosspostMessage> CrosspostTasks { get; set; } = new();

    internal NotifyCollectionChangedEventHandler CrosspostCollectionUpdated()
    {
        return (s, e) =>
        {
            _ = Bot.DatabaseClient.FullSyncDatabase();
        };
    }

    public async Task CrosspostWithRatelimit(DiscordChannel channel, DiscordMessage message)
    {
        if (!CrosspostRatelimits.ContainsKey(channel.Id))
        {
            _logger.LogDebug($"Initialized new crosspost ratelimit for '{channel.Id}'");
            CrosspostRatelimits.Add(channel.Id, new());
        }

        var r = CrosspostRatelimits[channel.Id];

        async Task Crosspost()
        {
            var task = channel.CrosspostMessageAsync(message);

            await Task.Delay(5000);
            if (!task.IsCompleted)
            {
                _logger.LogWarn($"Crosspost Ratelimit tripped for '{channel.Id}': {message.Id}");

                r.FirstPost = DateTime.UtcNow;
                r.PostsRemaining = 0;
            }

            while (!task.IsCompleted)
                task.Wait();

            return;
        }

        if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() <= 0)
        {
            _logger.LogDebug($"First crosspost for '{channel.Id}' was at {r.FirstPost.AddHours(1)}, resetting crosspost availability");
            r.FirstPost = DateTime.UtcNow;
            r.PostsRemaining = 10;
        }

        if (r.PostsRemaining > 0)
        {
            _logger.LogDebug($"{r.PostsRemaining} crossposts available for '{channel.Id}', allowing request");
            r.PostsRemaining--;
            await Crosspost();
            return;
        }

        if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
        {
            _logger.LogDebug($"No crossposts available for '{channel.Id}', waiting until {r.FirstPost.AddHours(1)} ({r.FirstPost.AddHours(1).GetTotalSecondsUntil()} seconds)");
            await Task.Delay(r.FirstPost.AddHours(1).GetTimespanUntil());
        }

        r.PostsRemaining = 9;
        r.FirstPost = DateTime.UtcNow;

        _logger.LogDebug($"Crossposts for '{channel.Id}' available again, allowing request. {r.PostsRemaining} requests remaining, first post at {r.FirstPost}.");
        await Crosspost();
        return;
    }
}
