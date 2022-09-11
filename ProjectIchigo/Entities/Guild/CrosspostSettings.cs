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

    internal NotifyCollectionChangedEventHandler CrosspostCollectionUpdated()
    {
        return (s, e) =>
        {
            _ = Bot.DatabaseClient.FullSyncDatabase();
        };
    }

    private bool QueueInitialized = false;
    private Dictionary<DiscordMessage, DiscordChannel> _queue = new();

    public async Task CrosspostQueue()
    {
        QueueInitialized = true;
        _logger.LogDebug($"Initializing crosspost queue for '{Parent.ServerId}'");

        while (true)
        {
            KeyValuePair<DiscordMessage, DiscordChannel> _;

            DiscordChannel channel;
            DiscordMessage message;

            try
            {
                while (!_queue.IsNotNullAndNotEmpty())
                    await Task.Delay(1000);

                _ = _queue.First();
                channel = _.Value;
                message = _.Key;
            }
            catch (Exception) 
            {
                _queue ??= new();
                continue; 
            }

            try
            {
                if (!CrosspostRatelimits.ContainsKey(channel.Id))
                {
                    _logger.LogDebug($"Initialized new crosspost ratelimit for '{channel.Id}'");
                    CrosspostRatelimits.Add(channel.Id, new());
                }

                var r = CrosspostRatelimits[channel.Id];

                _logger.LogDebug($"Crosspost Ratelimit '{channel.Id}' First Post: {r.FirstPost}");
                _logger.LogDebug($"Crosspost Ratelimit '{channel.Id}' Remaining Post: {r.PostsRemaining}");

                async Task Crosspost()
                {
                    if (message.Flags.HasValue && message.Flags.Value.HasMessageFlag(MessageFlags.Crossposted))
                        return;

                    r.PostsRemaining--;
                    var task = channel.CrosspostMessageAsync(message);

                    Stopwatch sw = new();
                    sw.Start();
                    while (!task.IsCompleted && sw.ElapsedMilliseconds < 3000)
                        Thread.Sleep(50);
                    sw.Stop();

                    _logger.LogDebug($"It took {sw.ElapsedMilliseconds}ms to process a crosspost");

                    if (!task.IsCompleted)
                    {
                        _logger.LogWarn($"Crosspost Ratelimit tripped for '{channel.Id}': {message.Id}");

                        r.FirstPost = DateTime.UtcNow;
                        r.PostsRemaining = 0;
                    }

                    while (!task.IsCompleted)
                        task.Wait();

                    _queue.Remove(message);
                    _logger.LogDebug($"Crossposted message in '{channel.Id}': {message.Id}");
                }

                void ResetLimits()
                {
                    r.PostsRemaining = 10;
                    r.FirstPost = DateTime.UtcNow;
                }

                if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() <= 0)
                {
                    _logger.LogDebug($"First crosspost for '{channel.Id}' was at {r.FirstPost.AddHours(1)}, resetting crosspost availability");
                    ResetLimits();
                }

                if (r.PostsRemaining > 0)
                {
                    _logger.LogDebug($"{r.PostsRemaining} crossposts available for '{channel.Id}', allowing request");
                    await Crosspost();
                    continue;
                }

                if (r.FirstPost.AddHours(1).GetTotalSecondsUntil() > 0)
                {
                    _logger.LogDebug($"No crossposts available for '{channel.Id}', waiting until {r.FirstPost.AddHours(1)} ({r.FirstPost.AddHours(1).GetTotalSecondsUntil()} seconds)");
                    await Task.Delay(r.FirstPost.AddHours(1).GetTimespanUntil());
                }

                ResetLimits();

                _logger.LogDebug($"Crossposts for '{channel.Id}' available again, allowing request. {r.PostsRemaining} requests remaining, first post at {r.FirstPost}.");
                await Crosspost();
                continue;
            }
            catch (Exception ex)
            {
                _queue.Remove(message);
                _logger.LogError($"Failed to process crosspost queue: {ex}");
            } 
        }
    }

    public async Task CrosspostWithRatelimit(DiscordChannel channel, DiscordMessage message)
    {
        if (!QueueInitialized)
            _ = CrosspostQueue();

        _queue.Add(message, channel);

        while (_queue.ContainsKey(message))
        {
            await Task.Delay(1000);
        }
    }
}
