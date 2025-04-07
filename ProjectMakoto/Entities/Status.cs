// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Status
{
    internal Status() { }

    public DateTime startupTime { get; internal set; } = DateTime.UtcNow;

    public string RunningVersion { get; internal set; }

    public bool DiscordInitialized { get; internal set; } = false;
    public bool DiscordGuildDownloadCompleted { get; internal set; } = false;
    public bool DiscordCommandsRegistered { get; internal set; } = false;

    public ulong TeamOwner { get; internal set; } = new();
    public IReadOnlyList<ulong> TeamMembers
        => this._TeamMembers.AsReadOnly();
    internal List<ulong> _TeamMembers { get; set; } = new();

    internal long DiscordDisconnections = 0;

    internal Config LoadedConfig { get; set; }

    private string? _CurrentAppHash { get; set; } = null;
    internal string CurrentAppHash
    {
        get
        {
            this._CurrentAppHash ??= HashingExtensions.ComputeSHA256Hash(new FileInfo(Assembly.GetExecutingAssembly().Location));
            return this._CurrentAppHash;
        }
    }

    public ExposedConfig SafeReadOnlyConfig
        => new(this.LoadedConfig);

    public class ExposedConfig(Config config)
    {
        public bool IsDev => config.IsDev;
        public bool AllowMoreThan100Guilds => config.AllowMoreThan100Guilds;

        public string SupportServerInvite = config.SupportServerInvite;
        public EmojiConfig Emojis = new(config);
        public DiscordConfig Discord = new(config);
        public ChannelsConfig Channels = new(config);
        public QuickChartConfig QuickChart = new(config);

        public sealed class DiscordConfig(Config config)
        {
            public ulong AssetsGuild => config.Discord.AssetsGuild;
            public ulong DevelopmentGuild => config.Discord.DevelopmentGuild;

            public uint MaxUploadSize => config.Discord.MaxUploadSize;
            public IReadOnlyList<string> DisabledCommands => config.Discord.DisabledCommands.ToList().AsReadOnly();
        }

        public sealed class ChannelsConfig(Config config)
        {
            public ulong GlobalBanAnnouncements => config.Channels.GlobalBanAnnouncements;
            public ulong GithubLog => config.Channels.GithubLog;
            public ulong News => config.Channels.News;

            public ulong GraphAssets => config.Channels.GraphAssets;
            public ulong PlaylistAssets => config.Channels.PlaylistAssets;
            public ulong UrlSubmissions => config.Channels.UrlSubmissions;
            public ulong OtherAssets => config.Channels.OtherAssets;

            public ulong ExceptionLog => config.Channels.ExceptionLog;
        }

        public sealed class EmojiConfig(Config config)
        {
            public ulong DisabledRepeat => config.Emojis.DisabledRepeat;
            public ulong DisabledShuffle => config.Emojis.DisabledShuffle;
            public ulong Paused => config.Emojis.Paused;
            public ulong DisabledPlay => config.Emojis.DisabledPlay;

            public ulong Error => config.Emojis.Error;

            public ulong CheckboxTicked => config.Emojis.CheckboxTicked;
            public ulong CheckboxUnticked => config.Emojis.CheckboxUnticked;

            public ulong PillOn => config.Emojis.PillOn;
            public ulong PillOff => config.Emojis.PillOff;

            public ulong QuestionMark => config.Emojis.QuestionMark;

            public ulong PrefixCommandDisabled => config.Emojis.PrefixCommandDisabled;
            public ulong PrefixCommandEnabled => config.Emojis.PrefixCommandEnabled;

            public ulong SlashCommand => config.Emojis.SlashCommand;
            public ulong MessageCommand => config.Emojis.MessageCommand;
            public ulong UserCommand => config.Emojis.UserCommand;

            public ulong Channel => config.Emojis.Channel;
            public ulong User => config.Emojis.User;
            public ulong VoiceState => config.Emojis.VoiceState;
            public ulong Message => config.Emojis.Message;
            public ulong Guild => config.Emojis.Guild;
            public ulong Invite => config.Emojis.Invite;
            public ulong In => config.Emojis.In;

            public ulong YouTube => config.Emojis.YouTube;
            public ulong SoundCloud => config.Emojis.SoundCloud;
            public ulong AbuseIPDB => config.Emojis.AbuseIPDB;
            public ulong Spotify => config.Emojis.Spotify;
        }

        public sealed class QuickChartConfig(Config config)
        {
            public string? Scheme => config.Secrets.QuickChart.Scheme;
            public string? Host = config.Secrets.QuickChart.Host;
            public int? Port = config.Secrets.QuickChart.Port;
        }
    }

    #region Legacy

    internal string DevelopmentServerInvite
    {
        get
        {
            return this.LoadedConfig.SupportServerInvite.IsNullOrWhiteSpace() ? "Invite not set." : this.LoadedConfig.SupportServerInvite;
        }
    }

    #endregion
}
