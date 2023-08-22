// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class Lavalink : RequiresParent<Guild>
{
    public Lavalink(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    DiscordGuild Guild { get; set; }

    public List<QueueInfo> SongQueue = new();

    public List<ulong> collectedSkips = new();
    public List<ulong> collectedDisconnectVotes = new();
    public List<ulong> collectedClearQueueVotes = new();

    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId
    {
        get => this._ChannelId;
        set
        {
            this._ChannelId = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_channel", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _CurrentVideo { get; set; } = "";
    public string CurrentVideo
    {
        get => this._CurrentVideo;
        set
        {
            this._CurrentVideo = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_currentvideo", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private long _CurrentVideoPosition { get; set; } = -1;
    public long CurrentVideoPosition
    {
        get => this._CurrentVideoPosition;
        set
        {
            this._CurrentVideoPosition = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_currentposition", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Repeat { get; set; } = false;
    public bool Repeat
    {
        get => this._Repeat;
        set
        {
            this._Repeat = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_repeat", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Shuffle { get; set; } = false;
    public bool Shuffle
    {
        get => this._Shuffle;
        set
        {
            this._Shuffle = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_shuffle", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _IsPaused { get; set; } = false;
    public bool IsPaused
    {
        get => this._IsPaused;
        set
        {
            this._IsPaused = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "lavalink_paused", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public sealed class QueueInfo
    {
        public QueueInfo(string VideoTitle, string Url, TimeSpan length, DiscordGuild guild, DiscordUser user)
        {
            this.VideoTitle = VideoTitle;
            this.Url = Url;
            this.Length = length;

            this.guild = guild;
            this.user = user;
        }

        public string VideoTitle { get; set; }
        public string Url { get; set; }

        public TimeSpan Length { get; set; }

        [JsonIgnore]
        public DiscordGuild guild { get; set; }

        [JsonIgnore]
        public DiscordUser user { get; set; }

        private ulong _GuildId = 0;
        public ulong GuildId
        {
            get => this.guild?.Id ?? this._GuildId;
            set
            {
                if (this.guild is not null)
                    throw new ArgumentException("Do not set this value when guild is already set.");

                this._GuildId = value;
            }
        }

        private ulong _UserId = 0;
        public ulong UserId
        {
            get => this.user?.Id ?? this._UserId;
            set
            {
                if (this.user is not null)
                    throw new ArgumentException("Do not set this value when user is already set.");

                this._UserId = value;
            }
        }
    }

    public bool Disposed { private set; get; } = false;
    public bool Initialized { private set; get; } = false;

    public void Dispose(Bot _bot, ulong Id, string reason)
    {
        this.Disposed = true;

        _logger.LogDebug("Disposed Player for {Id}. ({reason})", Id, reason);

        _bot.Guilds[Id].MusicModule = new(this.Bot, this.Parent);
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
                                        _bot.Guilds[e.Guild.Id].MusicModule = new(this.Bot, this.Parent);
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

                    while ((guildPlayer.CurrentTrack is not null || _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Count <= 0) && !TrackEnded && !this.Disposed)
                    {
                        if (guildPlayer.CurrentTrack is null && _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Count <= 0)
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
                        skipSongs = _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.IndexOf(LastPlayedTrack) + 1;

                        if (skipSongs >= _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Count)
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
                        _ = _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Remove(Track);
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
                        _ = _bot.Guilds[this.Guild.Id].MusicModule.SongQueue.Remove(Track);
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
