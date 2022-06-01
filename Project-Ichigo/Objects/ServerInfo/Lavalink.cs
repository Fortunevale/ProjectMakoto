namespace Project_Ichigo.Objects;

internal class Lavalink
{
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

    public void Dispose()
    {
        this.Disposed = true;
    }

    public void QueueHandler(Bot _bot, DiscordClient sender, LavalinkNodeConnection nodeConnection, LavalinkGuildConnection guildConnection)
    {
        Task.Run(async () =>
        {
            if (Initialized || Disposed)
                return;

            Initialized = true;

            LogDebug($"Initializing Player for {guildConnection.Guild.Id}..");

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



                        if (UserAmount <= 1)
                            _ = Task.Delay(30000, VoiceUpdateTokenSource.Token).ContinueWith(x =>
                            {
                                if (!x.IsCompletedSuccessfully)
                                    return;

                                if (UserAmount <= 1)
                                {
                                    _bot._guilds.List[e.Guild.Id].Lavalink.Dispose();
                                    _bot._guilds.List[e.Guild.Id].Lavalink = new();
                                }
                            });
                    }
                }).Add(_bot._watcher);
            }

            LogDebug($"Initializing VoiceStateUpdated Event for {guildConnection.Guild.Id}..");
            sender.VoiceStateUpdated += VoiceStateUpdated;

            while (true)
            {
                while ((guildConnection.CurrentState.CurrentTrack is not null || _bot._guilds.List[guildConnection.Guild.Id].Lavalink.SongQueue.Count <= 0) && !Disposed)
                    await Task.Delay(1000);

                if (Disposed)
                {
                    LogDebug($"Destroying Player for {guildConnection.Guild.Id}..");
                    sender.VoiceStateUpdated -= VoiceStateUpdated;
                    return;
                }

                Lavalink.QueueInfo Track;

                if (_bot._guilds.List[guildConnection.Guild.Id].Lavalink.Shuffle)
                    Track = _bot._guilds.List[guildConnection.Guild.Id].Lavalink.SongQueue.OrderBy(_ => Guid.NewGuid()).ToList().First();
                else
                    Track = _bot._guilds.List[guildConnection.Guild.Id].Lavalink.SongQueue.ToList().First();

                _bot._guilds.List[guildConnection.Guild.Id].Lavalink.collectedSkips.Clear();

                var loadResult = await nodeConnection.Rest.GetTracksAsync(Track.Url, LavalinkSearchType.Plain);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    _bot._guilds.List[guildConnection.Guild.Id].Lavalink.SongQueue.Remove(Track);
                    continue;
                }

                var loadedTrack = loadResult.Tracks.First();

                guildConnection = nodeConnection.GetGuildConnection(guildConnection.Guild);

                if (guildConnection is not null)
                {
                    await guildConnection.PlayAsync(loadedTrack);
                }
                else
                {
                    this.Dispose();
                    continue;
                }

                _bot._guilds.List[guildConnection.Guild.Id].Lavalink.SongQueue.Remove(Track);
            }
        }).Add(_bot._watcher);
    }
}
