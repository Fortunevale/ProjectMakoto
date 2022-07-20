namespace ProjectIchigo.Entities;

internal class Lavalink
{
    DiscordGuild Guild { get; set; }

    public List<QueueInfo> SongQueue = new();

    public List<ulong> collectedSkips = new();
    public List<ulong> collectedDisconnectVotes = new();
    public List<ulong> collectedClearQueueVotes = new();

    public bool Repeat = false;
    public bool Shuffle = false;

    public bool IsPaused = false;

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

        _bot._guilds.List[Id].Lavalink = new();
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
                                        _bot._guilds.List[e.Guild.Id].Lavalink.Dispose(_bot, e.Guild.Id, "No users");
                                        _bot._guilds.List[e.Guild.Id].Lavalink = new();
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
                            }
                        }
                    }).Add(_bot._watcher);
                }

                _logger.LogDebug($"Initializing VoiceStateUpdated Event for {Guild.Id}..");
                sender.VoiceStateUpdated += VoiceStateUpdated;

                QueueInfo LastPlayedTrack = null;

                while (true)
                {
                    int WaitSeconds = 30;

                    while ((guildConnection.CurrentState.CurrentTrack is not null || _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Count <= 0) && !Disposed)
                    {
                        if (guildConnection.CurrentState.CurrentTrack is null && _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Count <= 0)
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

                        _ = guildConnection.DisconnectAsync();
                        return;
                    }

                    Lavalink.QueueInfo Track;

                    int skipSongs = 0;

                    if (LastPlayedTrack is not null && _bot._guilds.List[Guild.Id].Lavalink.Repeat && _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Contains(LastPlayedTrack))
                    {
                        skipSongs = _bot._guilds.List[Guild.Id].Lavalink.SongQueue.IndexOf(LastPlayedTrack) + 1;

                        if (skipSongs >= _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Count)
                            skipSongs = 0;
                    }

                    if (_bot._guilds.List[Guild.Id].Lavalink.Shuffle)
                        Track = _bot._guilds.List[Guild.Id].Lavalink.SongQueue.OrderBy(_ => Guid.NewGuid()).ToList().First();
                    else
                        Track = _bot._guilds.List[Guild.Id].Lavalink.SongQueue.ToList().Skip(skipSongs).First();

                    LastPlayedTrack = Track;

                    _bot._guilds.List[Guild.Id].Lavalink.collectedSkips.Clear();

                    var loadResult = await nodeConnection.Rest.GetTracksAsync(Track.Url, LavalinkSearchType.Plain);

                    if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                    {
                        if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                            _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Remove(Track);

                        continue;
                    }

                    var loadedTrack = loadResult.Tracks.First();

                    guildConnection = nodeConnection.GetGuildConnection(Guild);

                    if (guildConnection is not null)
                    {
                        await guildConnection.PlayAsync(loadedTrack);
                    }
                    else
                    {
                        this.Dispose(_bot, Guild.Id, "guildConnection is null");
                        continue;
                    }

                    if (!_bot._guilds.List[Guild.Id].Lavalink.Repeat)
                        _bot._guilds.List[Guild.Id].Lavalink.SongQueue.Remove(Track);
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
