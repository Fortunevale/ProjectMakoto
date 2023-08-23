// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class Lavalink : RequiresParent<Guild>
{
    public Lavalink(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    public void Reset()
    {
        this.SongQueue = Array.Empty<QueueInfo>();
        this.ChannelId = 0;
        this.CurrentVideo = null;
        this.CurrentVideoPosition = -1;
        this.Repeat = false;
        this.Shuffle = false;
        this.IsPaused = false;

        this.Parent.MusicModule = new(this.Parent.Bot, this.Parent);
    }

    private DiscordGuild Guild { get; set; }

    public List<ulong> collectedSkips = new();
    public List<ulong> collectedDisconnectVotes = new();
    public List<ulong> collectedClearQueueVotes = new();

    [ColumnName("lavalink_queue"), ColumnType(ColumnTypes.LongText), Collation("utf8_unicode_ci"), Default("[]")]
    public QueueInfo[] SongQueue
    {
        get => JsonConvert.DeserializeObject<QueueInfo[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "lavalink_queue", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_queue", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_channel"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong ChannelId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "lavalink_channel", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_channel", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_currentvideo"), ColumnType(ColumnTypes.Text), Collation("utf8_unicode_ci"), Nullable]
    public string? CurrentVideo
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "lavalink_currentvideo", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_currentvideo", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_currentposition"), ColumnType(ColumnTypes.BigInt), Default("-1")]
    public long CurrentVideoPosition
    {
        get => this.Bot.DatabaseClient.GetValue<long>("guilds", "serverid", this.Parent.Id, "lavalink_currentposition", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_currentposition", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_repeat"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool Repeat
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "lavalink_repeat", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_repeat", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_shuffle"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool Shuffle
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "lavalink_shuffle", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_shuffle", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("lavalink_paused"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool IsPaused
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "lavalink_paused", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_paused", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    public sealed class QueueInfo
    {
        public QueueInfo(string VideoTitle, string Url, TimeSpan length, DiscordGuild guild, DiscordUser user)
        {
            this.VideoTitle = VideoTitle;
            this.Url = Url;
            this.Length = length;

            this.GuildId = guild?.Id ?? 0;
            this.UserId = user?.Id ?? 0;
        }

        public string UUID { get; set; } = Guid.NewGuid().ToString();

        public string VideoTitle { get; set; }
        public string Url { get; set; }

        public TimeSpan Length { get; set; }

        public ulong GuildId = 0;
        public ulong UserId = 0;
    }

    public bool Disposed { private set; get; } = false;
    public bool Initialized { private set; get; } = false;

    public void Dispose(Bot _bot, ulong Id, string reason)
    {
        this.Disposed = true;

        _logger.LogDebug("Disposed Player for {Id}. ({reason})", Id, reason);

        _bot.Guilds[Id].MusicModule.Reset();
    }

    public void QueueHandler(Bot _bot, DiscordClient sender, LavalinkSession session, LavalinkGuildPlayer guildPlayer)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                if (this.Initialized || this.Disposed)
                    return;

                this.Initialized = true;

                this.Guild = guildPlayer.Guild;

                _logger.LogDebug("Initializing Player for {Guild}..", this.Guild.Id);

                var UserAmount = guildPlayer.Channel.Users.Count;
                CancellationTokenSource VoiceUpdateTokenSource = new();
                async Task VoiceStateUpdated(DiscordClient s, VoiceStateUpdateEventArgs e)
                {
                    if (e.Guild?.Id != this.Guild?.Id)
                        return;

                    _ = Task.Run(async () =>
                    {
                        if (e.Channel?.Id == guildPlayer.Channel?.Id || e.Before?.Channel?.Id == guildPlayer.Channel?.Id)
                        {
                            VoiceUpdateTokenSource.Cancel();
                            VoiceUpdateTokenSource = new();

                            UserAmount = e.Channel is not null ? e.Channel.Users.Count : e.Guild.Channels.First(x => x.Key == e.Before.Channel.Id).Value.Users.Count;

                            _logger.LogTrace("UserAmount updated to {UserAmount} for {Guild}", UserAmount, this.Guild.Id);

                            if (UserAmount <= 1)
                                _ = Task.Delay(30000, VoiceUpdateTokenSource.Token).ContinueWith(x =>
                                {
                                    if (!x.IsCompletedSuccessfully)
                                        return;

                                    if (UserAmount <= 1)
                                    {
                                        _bot.Guilds[e.Guild.Id].MusicModule.Dispose(_bot, e.Guild.Id, "No users");
                                        _bot.Guilds[e.Guild.Id].MusicModule.Reset();
                                    }
                                });
                        }
                    }).Add(_bot);

                    _ = Task.Run(async () =>
                    {
                        if (e.User.Id == sender.CurrentUser.Id)
                        {
                            if (e.After is null || e.After.Channel is null)
                            {
                                _ = guildPlayer.DisconnectAsync();
                                this.Dispose(_bot, e.Guild.Id, "Disconnected");
                                return;
                            }
                        }
                    }).Add(_bot);
                }

                async Task StateUpdated(LavalinkGuildPlayer sender, LavalinkPlayerStateUpdateEventArgs e)
                {
                    this.CurrentVideo = (sender.CurrentTrack?.Info?.Uri ?? new UriBuilder().Uri).ToString();
                    this.CurrentVideoPosition = (Convert.ToInt64(e.State?.Position.TotalSeconds ?? -1d));
                }

                var TrackEnded = false;
                async Task TrackEnd(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e)
                {
                    TrackEnded = true;
                }

                _logger.LogDebug("Initializing VoiceStateUpdated Event for {Guild}..", this.Guild.Id);
                sender.VoiceStateUpdated += VoiceStateUpdated;

                _logger.LogDebug("Initializing PlayerUpdated Event for {Guild}..", this.Guild.Id);
                guildPlayer.StateUpdated += StateUpdated;
                guildPlayer.TrackEnded += TrackEnd;

                QueueInfo LastPlayedTrack = null;

                while (true)
                {
                    var WaitSeconds = 30;

                    while ((guildPlayer.CurrentTrack is not null || _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Length <= 0) && !TrackEnded && !this.Disposed)
                    {
                        if (guildPlayer.CurrentTrack is null && _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Length <= 0)
                        {
                            WaitSeconds--;

                            if (WaitSeconds <= 0)
                                break;
                        }

                        await Task.Delay(1000);
                    }

                    if (WaitSeconds <= 0)
                        this.Dispose(_bot, this.Guild.Id, "Time out, nothing playing");

                    if (this.Disposed)
                    {
                        _logger.LogDebug("Destroying Player for {Guild}..", this.Guild.Id);
                        sender.VoiceStateUpdated -= VoiceStateUpdated;
                        guildPlayer.StateUpdated -= StateUpdated;
                        guildPlayer.TrackEnded -= TrackEnd;

                        _ = guildPlayer.DisconnectAsync();
                        return;
                    }

                    TrackEnded = false;
                    QueueInfo Track;

                    var skipSongs = 0;

                    if (LastPlayedTrack is not null && _bot.Guilds[this.Guild.Id].MusicModule.Repeat && _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Contains(LastPlayedTrack))
                    {
                        skipSongs = Array.IndexOf(_bot.Guilds[this.Guild.Id].MusicModule.SongQueue, LastPlayedTrack) + 1;

                        if (skipSongs >= _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Length)
                            skipSongs = 0;
                    }

                    Track = _bot.Guilds[this.Guild.Id].MusicModule.Shuffle
                        ? _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.OrderBy(_ => Guid.NewGuid()).ToList().First()
                        : _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.ToList().Skip(skipSongs).First();

                    LastPlayedTrack = Track;

                    _bot.Guilds[this.Guild.Id].MusicModule.collectedSkips.Clear();

                    var loadResult = await session.LoadTracksAsync(LavalinkSearchType.Plain, Track.Url);

                    if (loadResult.LoadType is LavalinkLoadResultType.Error or LavalinkLoadResultType.Empty)
                    {
                        _bot.Guilds[this.Guild.Id].MusicModule.SongQueue = _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Remove(x => x.UUID, Track);
                        continue;
                    }

                    var loadedTrack = loadResult.LoadType switch
                    {
                        LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                        LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                        LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                        _ => throw new InvalidOperationException("Unexpected load result type.")
                    };

                    guildPlayer = session.GetGuildPlayer(this.Guild);
                    this.ChannelId = guildPlayer.Channel.Id;

                    if (guildPlayer is not null)
                    {
                        _ = await guildPlayer.PlayAsync(loadedTrack);
                    }
                    else
                    {
                        this.Dispose(_bot, this.Guild.Id, "guildConnection is null");
                        continue;
                    }

                    if (!_bot.Guilds[this.Guild.Id].MusicModule.Repeat)
                        _bot.Guilds[this.Guild.Id].MusicModule.SongQueue = _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Remove(x => x.UUID, Track);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while trying to handle music queue", ex);

                _ = guildPlayer.DisconnectAsync();
                this.Dispose(_bot, this.Guild.Id, "Exception");
                throw;
            }
        }).Add(_bot);
    }
}
