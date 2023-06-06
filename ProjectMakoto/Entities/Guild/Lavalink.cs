// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Lavalink
{
    public Lavalink(Guild guild)
    {
        this.Parent = guild;
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
        get => this._ChannelId;
        set
        {
            this._ChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_channel", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private string _CurrentVideo { get; set; } = "";
    public string CurrentVideo
    {
        get => this._CurrentVideo;
        set
        {
            this._CurrentVideo = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_currentvideo", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private long _CurrentVideoPosition { get; set; } = -1;
    public long CurrentVideoPosition
    {
        get => this._CurrentVideoPosition;
        set
        {
            this._CurrentVideoPosition = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_currentposition", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Repeat { get; set; } = false;
    public bool Repeat
    {
        get => this._Repeat;
        set
        {
            this._Repeat = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_repeat", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _Shuffle { get; set; } = false;
    public bool Shuffle
    {
        get => this._Shuffle;
        set
        {
            this._Shuffle = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_shuffle", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _IsPaused { get; set; } = false;
    public bool IsPaused
    {
        get => this._IsPaused;
        set
        {
            this._IsPaused = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "lavalink_paused", value, Bot.DatabaseClient.mainDatabaseConnection);
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

        _bot.guilds[Id].MusicModule = new(this.Parent);
    }

    public void QueueHandler(Bot _bot, DiscordClient sender, LavalinkNodeConnection nodeConnection, LavalinkGuildConnection guildConnection)
    {
        Task.Run(async () =>
        {
            try
            {
                if (this.Initialized || this.Disposed)
                    return;

                this.Initialized = true;

                this.Guild = guildConnection.Guild;

                _logger.LogDebug("Initializing Player for {Guild}..", this.Guild.Id);

                int UserAmount = guildConnection.Channel.Users.Count;
                CancellationTokenSource VoiceUpdateTokenSource = new();
                async Task VoiceStateUpdated(DiscordClient s, VoiceStateUpdateEventArgs e)
                {
                    if (e.Guild?.Id != this.Guild?.Id)
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

                            _logger.LogTrace("UserAmount updated to {UserAmount} for {Guild}", UserAmount, this.Guild.Id);

                            if (UserAmount <= 1)
                                _ = Task.Delay(30000, VoiceUpdateTokenSource.Token).ContinueWith(x =>
                                {
                                    if (!x.IsCompletedSuccessfully)
                                        return;

                                    if (UserAmount <= 1)
                                    {
                                        _bot.guilds[e.Guild.Id].MusicModule.Dispose(_bot, e.Guild.Id, "No users");
                                        _bot.guilds[e.Guild.Id].MusicModule = new(this.Parent);
                                    }
                                });
                        }
                    }).Add(_bot.watcher);

                    Task.Run(async () =>
                    {
                        if (e.User.Id == sender.CurrentUser.Id)
                        {
                            if (e.After is null || e.After.Channel is null)
                            {
                                _ = guildConnection.DisconnectAsync();
                                Dispose(_bot, e.Guild.Id, "Disconnected");
                                return;
                            }
                        }
                    }).Add(_bot.watcher);
                }

                async Task PlayerUpdated(LavalinkGuildConnection sender, PlayerUpdateEventArgs e)
                {
                    this.CurrentVideo = (e.Player?.CurrentState?.CurrentTrack?.Uri ?? new UriBuilder().Uri).ToString();
                    this.CurrentVideoPosition = (Convert.ToInt64(e.Player?.CurrentState?.PlaybackPosition.TotalSeconds ?? -1d));
                }

                _logger.LogDebug("Initializing VoiceStateUpdated Event for {Guild}..", this.Guild.Id);
                sender.VoiceStateUpdated += VoiceStateUpdated;

                _logger.LogDebug("Initializing PlayerUpdated Event for {Guild}..", this.Guild.Id);
                guildConnection.PlayerUpdated += PlayerUpdated;

                QueueInfo LastPlayedTrack = null;

                while (true)
                {
                    int WaitSeconds = 30;

                    while ((guildConnection.CurrentState.CurrentTrack is not null || _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Count <= 0) && !this.Disposed)
                    {
                        if (guildConnection.CurrentState.CurrentTrack is null && _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Count <= 0)
                        {
                            WaitSeconds--;

                            if (WaitSeconds <= 0)
                                break;
                        }

                        await Task.Delay(1000);
                    }

                    if (WaitSeconds <= 0)
                        Dispose(_bot, this.Guild.Id, "Time out, nothing playing");

                    if (this.Disposed)
                    {
                        _logger.LogDebug("Destroying Player for {Guild}..", this.Guild.Id);
                        sender.VoiceStateUpdated -= VoiceStateUpdated;
                        guildConnection.PlayerUpdated -= PlayerUpdated;

                        _ = guildConnection.DisconnectAsync();
                        return;
                    }

                    Lavalink.QueueInfo Track;

                    int skipSongs = 0;

                    if (LastPlayedTrack is not null && _bot.guilds[this.Guild.Id].MusicModule.Repeat && _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Contains(LastPlayedTrack))
                    {
                        skipSongs = _bot.guilds[this.Guild.Id].MusicModule.SongQueue.IndexOf(LastPlayedTrack) + 1;

                        if (skipSongs >= _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Count)
                            skipSongs = 0;
                    }

                    if (_bot.guilds[this.Guild.Id].MusicModule.Shuffle)
                        Track = _bot.guilds[this.Guild.Id].MusicModule.SongQueue.OrderBy(_ => Guid.NewGuid()).ToList().First();
                    else
                        Track = _bot.guilds[this.Guild.Id].MusicModule.SongQueue.ToList().Skip(skipSongs).First();

                    LastPlayedTrack = Track;

                    _bot.guilds[this.Guild.Id].MusicModule.collectedSkips.Clear();

                    var loadResult = await nodeConnection.Rest.GetTracksAsync(Track.Url, LavalinkSearchType.Plain);

                    if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                    {
                        if (loadResult.LoadResultType is LavalinkLoadResultType.NoMatches or LavalinkLoadResultType.LoadFailed)
                            _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Remove(Track);

                        continue;
                    }

                    var loadedTrack = loadResult.Tracks.First();

                    guildConnection = nodeConnection.GetGuildConnection(this.Guild);
                    this.ChannelId = guildConnection.Channel.Id;

                    if (guildConnection is not null)
                    {
                        await guildConnection.PlayAsync(loadedTrack);
                    }
                    else
                    {
                        Dispose(_bot, this.Guild.Id, "guildConnection is null");
                        continue;
                    }

                    if (!_bot.guilds[this.Guild.Id].MusicModule.Repeat)
                        _bot.guilds[this.Guild.Id].MusicModule.SongQueue.Remove(Track);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while trying to handle music queue", ex);

                _ = guildConnection.DisconnectAsync();
                Dispose(_bot, this.Guild.Id, "Exception");
                throw;
            }
        }).Add(_bot.watcher);
    }
}
