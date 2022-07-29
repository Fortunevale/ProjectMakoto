namespace ProjectIchigo.Entities;

public class Lavalink
{
    public Lavalink(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    DiscordGuild Guild { get; set; }

    public List<QueueInfo> SongQueue = new();

    public List<ulong> collectedSkips = new();
    public List<ulong> collectedDisconnectVotes = new();
    public List<ulong> collectedClearQueueVotes = new();

    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId
    {
        get => _ChannelId;
        set
        {
            _ChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_channel", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
    
    private string _CurrentVideo { get; set; } = "";
    public string CurrentVideo
    {
        get => _CurrentVideo;
        set
        {
            _CurrentVideo = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_currentvideo", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
    
    private long _CurrentVideoPosition { get; set; } = -1;
    public long CurrentVideoPosition
    {
        get => _CurrentVideoPosition;
        set
        {
            _CurrentVideoPosition = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_currentposition", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Repeat { get; set; } = false;
    public bool Repeat
    {
        get => _Repeat;
        set
        {
            _Repeat = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_repeat", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Shuffle { get; set; } = false;
    public bool Shuffle
    {
        get => _Shuffle;
        set
        {
            _Shuffle = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_shuffle", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _IsPaused { get; set; } = false;
    public bool IsPaused
    {
        get => _IsPaused;
        set
        {
            _IsPaused = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "lavalink_paused", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public class QueueInfo
    {
        public QueueInfo(string VideoTitle, string Url, DiscordGuild guild, DiscordUser user)
        {
            this.VideoTitle = VideoTitle;
            this.Url = Url;
            this.guild = guild;
            this.user = user;
        }

        public string VideoTitle { get; set; }
        public string Url { get; set; }
        public DiscordGuild guild { get; set; }
        public DiscordUser user { get; set; }
    }

    public bool Disposed { private set; get; } = false;
    public bool Initialized { private set; get; } = false;

    public void Dispose(Bot _bot, ulong Id, string reason)
    {
        this.Disposed = true;

        _logger.LogDebug($"Disposed Player for {Id}. ({reason})");

        _bot._guilds[Id].Lavalink = new(Parent);
    }

    public void QueueHandler(Bot _bot, DiscordClient sender, LavalinkNodeConnection nodeConnection, LavalinkGuildConnection guildConnection)
    {
        Task.Run(async () =>
        {
            try
            {
                if (Initialized || Disposed)
                    return;

                Initialized = true;

                this.Guild = guildConnection.Guild;

                _logger.LogDebug($"Initializing Player for {Guild.Id}..");

                int UserAmount = 0;
                CancellationTokenSource VoiceUpdateTokenSource = new();
                async Task VoiceStateUpdated(DiscordClient s, VoiceStateUpdateEventArgs e)
                {
                    if (e.Guild?.Id != Guild?.Id)
                        return;

                    Task.Run(async () =>
                    {
                        if (e.Channel?.Id == guildConnection.Channel?.Id || e.Before?.Channel?.Id == guildConnection.Channel?.Id)
                        {
                            VoiceUpdateTokenSource.Cancel();
                            VoiceUpdateTokenSource = new();

                            if (e.Channel is not null)
                                UserAmount = e.Channel.Users.Count;
                            else
                                UserAmount = e.Guild.Channels.First(x => x.Key == e.Before.Channel.Id).Value.Users.Count;

                            _logger.LogTrace($"UserAmount updated to {UserAmount} for {Guild.Id}");

                            if (UserAmount <= 1)
                                _ = Task.Delay(30000, VoiceUpdateTokenSource.Token).ContinueWith(x =>
                                {
                                    if (!x.IsCompletedSuccessfully)
                                        return;

                                    if (UserAmount <= 1)
                                    {
                                        _bot._guilds[e.Guild.Id].Lavalink.Dispose(_bot, e.Guild.Id, "No users");
                                        _bot._guilds[e.Guild.Id].Lavalink = new(Parent);
                                    }
                                });
                        }
                    }).Add(_bot._watcher);

                    Task.Run(async () =>
                    {
                        if (e.User.Id == sender.CurrentUser.Id)
                        {
                            if (e.After is null || e.After.Channel is null)
                            {
                                _ = guildConnection.DisconnectAsync();
                                this.Dispose(_bot, e.Guild.Id, "Disconnected");
                                return;
                            }

                            if (e.Before?.Channel != e.After?.Channel)
                            {
                                _logger.LogTrace($"Switched Channel on {Guild.Id}");

                                var conn = nodeConnection.GetGuildConnection(e.Guild);

                                LavalinkTrack? track = conn?.CurrentState?.CurrentTrack;
                                TimeSpan? position = conn?.CurrentState?.PlaybackPosition;

                                if (track is null || position is null)
                                {
                                    _ = guildConnection.DisconnectAsync();
                                    this.Dispose(_bot, e.Guild.Id, "Error occured carrying on with playback after channel switch");
                                    return;
                                }

                                if (conn is null)
                                {
                                    _ = guildConnection.DisconnectAsync();
                                    this.Dispose(_bot, e.Guild.Id, "Conn is null");
                                    return;
                                }

                                await conn.StopAsync();

                                await Task.Delay(1000);

                                await conn.PlayAsync(track);
                                await conn.SeekAsync((TimeSpan)position);
                                guildConnection = nodeConnection.GetGuildConnection(Guild);
                                ChannelId = guildConnection.Channel.Id;
                            }
                        }
                    }).Add(_bot._watcher);
                }

                async Task PlayerUpdated(LavalinkGuildConnection sender, PlayerUpdateEventArgs e)
                {
                    CurrentVideo = (e.Player?.CurrentState?.CurrentTrack?.Uri ?? new UriBuilder().Uri).ToString();
                    CurrentVideoPosition = (Convert.ToInt64(e.Player?.CurrentState?.PlaybackPosition.TotalSeconds ?? -1d));
                }

                _logger.LogDebug($"Initializing VoiceStateUpdated Event for {Guild.Id}..");
                sender.VoiceStateUpdated += VoiceStateUpdated;

                _logger.LogDebug($"Initializing PlayerUpdated Event for {Guild.Id}..");
                guildConnection.PlayerUpdated += PlayerUpdated;

                QueueInfo LastPlayedTrack = null;

                while (true)
                {
                    int WaitSeconds = 30;

                    while ((guildConnection.CurrentState.CurrentTrack is not null || _bot._guilds[Guild.Id].Lavalink.SongQueue.Count <= 0) && !Disposed)
                    {
                        if (guildConnection.CurrentState.CurrentTrack is null && _bot._guilds[Guild.Id].Lavalink.SongQueue.Count <= 0)
                        {
                            WaitSeconds--;

                            if (WaitSeconds <= 0)
                                break;
                        }

                        await Task.Delay(1000);
                    }

                    if (WaitSeconds <= 0)
                        this.Dispose(_bot, Guild.Id, "Time out, nothing playing");

                    if (Disposed)
                    {
                        _logger.LogDebug($"Destroying Player for {Guild.Id}..");
                        sender.VoiceStateUpdated -= VoiceStateUpdated;
                        guildConnection.PlayerUpdated -= PlayerUpdated;

                        _ = guildConnection.DisconnectAsync();
                        return;
                    }

                    Lavalink.QueueInfo Track;

                    int skipSongs = 0;

                    if (LastPlayedTrack is not null && _bot._guilds[Guild.Id].Lavalink.Repeat && _bot._guilds[Guild.Id].Lavalink.SongQueue.Contains(LastPlayedTrack))
                    {
                        skipSongs = _bot._guilds[Guild.Id].Lavalink.SongQueue.IndexOf(LastPlayedTrack) + 1;

                        if (skipSongs >= _bot._guilds[Guild.Id].Lavalink.SongQueue.Count)
                            skipSongs = 0;
                    }

                    if (_bot._guilds[Guild.Id].Lavalink.Shuffle)
                        Track = _bot._guilds[Guild.Id].Lavalink.SongQueue.OrderBy(_ => Guid.NewGuid()).ToList().First();
                    else
                        Track = _bot._guilds[Guild.Id].Lavalink.SongQueue.ToList().Skip(skipSongs).First();

                    LastPlayedTrack = Track;

                    _bot._guilds[Guild.Id].Lavalink.collectedSkips.Clear();

                    var loadResult = await nodeConnection.Rest.GetTracksAsync(Track.Url, LavalinkSearchType.Plain);

                    if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                    {
                        if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                            _bot._guilds[Guild.Id].Lavalink.SongQueue.Remove(Track);

                        continue;
                    }

                    var loadedTrack = loadResult.Tracks.First();

                    guildConnection = nodeConnection.GetGuildConnection(Guild);
                    ChannelId = guildConnection.Channel.Id;

                    if (guildConnection is not null)
                    {
                        await guildConnection.PlayAsync(loadedTrack);
                    }
                    else
                    {
                        this.Dispose(_bot, Guild.Id, "guildConnection is null");
                        continue;
                    }

                    if (!_bot._guilds[Guild.Id].Lavalink.Repeat)
                        _bot._guilds[Guild.Id].Lavalink.SongQueue.Remove(Track);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occured while trying to handle music queue", ex);

                _ = guildConnection.DisconnectAsync();
                this.Dispose(_bot, Guild.Id, "Exception");
                throw;
            }
        }).Add(_bot._watcher);
    }
}
