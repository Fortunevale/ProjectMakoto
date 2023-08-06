// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Config
{
    public void Save(int retry = 0)
    {
        try
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Include }));
        }
        catch (Exception)
        {
            if (retry > 10)
                return;

            Thread.Sleep(500);
            this.Save(retry + 1);
        }
    }

    public bool IsDev = false;
    public bool AllowMoreThan100Guilds = false;

    public bool MonitorSystemStatus = true;

    public bool EnablePlugins = false;

    public string SupportServerInvite = "";

    public DiscordConfig Discord = new();
    public ChannelsConfig Channels = new();
    public EmojiConfig Emojis = new();
    public AccountIdsConfig Accounts = new();
    public SecretsConfig Secrets = new();
    public DontModifyConfig DontModify = new();

    public Dictionary<string, PluginInfo> PluginCache = new();
    public Dictionary<string, object> PluginData = new();

    public sealed class DiscordConfig
    {
        public ulong AssetsGuild = 0;
        public ulong DevelopmentGuild = 0;

        public uint MaxUploadSize = 8388608;
        public List<string> DisabledCommands = new();
    }

    public sealed class ChannelsConfig
    {
        public ulong GlobalBanAnnouncements = 0;
        public ulong GithubLog = 0;
        public ulong News = 0;

        public ulong GraphAssets = 0;
        public ulong PlaylistAssets = 0;
        public ulong UrlSubmissions = 0;
        public ulong OtherAssets = 0;

        public ulong ExceptionLog = 0;
    }

    public sealed class EmojiConfig
    {
        public string[] JoinEvent = { "🙋‍", "🙋‍" };
        public string Cuddle = "🅿";
        public string Kiss = "🅿";
        public string Slap = "🅿";
        public string Proud = "🅿";
        public string Hug = "🅿";

        public ulong DisabledRepeat = 0;
        public ulong DisabledShuffle = 0;
        public ulong Paused = 0;
        public ulong DisabledPlay = 0;

        public ulong Error = 0;

        public ulong CheckboxTicked = 0;
        public ulong CheckboxUnticked = 0;

        public ulong PillOn = 0;
        public ulong PillOff = 0;

        public ulong QuestionMark = 0;

        public ulong PrefixCommandDisabled = 0;
        public ulong PrefixCommandEnabled = 0;

        public ulong SlashCommand = 0;
        public ulong MessageCommand = 0;
        public ulong UserCommand = 0;

        public ulong Channel = 0;
        public ulong User = 0;
        public ulong VoiceState = 0;
        public ulong Message = 0;
        public ulong Guild = 0;
        public ulong Invite = 0;
        public ulong In = 0;

        public ulong YouTube = 0;
        public ulong SoundCloud = 0;
        public ulong AbuseIPDB = 0;
    }

    public sealed class AccountIdsConfig
    {
        public ulong Disboard = 302050872383242240;
    }

    public sealed class SecretsConfig
    {
        public string KawaiiRedToken = "";
        public string AbuseIpDbToken = "";
        public string LibreTranslateHost = "127.0.0.1";

        public DiscordSecrets Discord = new();
        public TelegramSecrets Telegram = new();
        public GithubSecrets Github = new();
        public DatabaseSecrets Database = new();
        public LavalinkSecrets Lavalink = new();

        public sealed class DiscordSecrets
        {
            public string Token = "";
        }

        public sealed class TelegramSecrets
        {
            public string Token = "";
        }

        public sealed class GithubSecrets
        {
            public string Token = "";

            public DateTimeOffset TokenExperiation = new(0001, 01, 01, 15, 00, 00, TimeSpan.Zero);
            public string Username = "";
            public string Repository = "";
            public string? Branch = "";
            public string TokenLeakRepoOwner = "";
            public string TokenLeakRepo = "";
        }

        public sealed class DatabaseSecrets
        {
            public string Host = "127.0.0.1";
            public int Port = 3306;
            public string Username = "";
            public string Password = "";

            public string MainDatabaseName = "";
            public string GuildDatabaseName = "";
        }

        public sealed class LavalinkSecrets
        {
            public string Host = "127.0.0.1";
            public int Port = 2333;
            public string Password = "";
        }
    }

    public sealed class DontModifyConfig
    {
        public string LastStartedVersion = "UNIDENTIFIED";
        public string LastKnownHash = "";
    }

    public sealed class PluginInfo
    {
        public string? LastKnownHash = null;
        public Dictionary<string, string> CompiledCommands = new();
    }
}
