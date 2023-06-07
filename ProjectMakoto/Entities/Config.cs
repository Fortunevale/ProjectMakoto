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
            Save(retry + 1);
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
        public uint MaxUploadSize = 8388608;
        public List<string> DisabledCommands = new();
    }

    public sealed class ChannelsConfig
    {
        public ulong GlobalBanAnnouncements = 0;
        public ulong GithubLog = 0;
        public ulong News = 0;

        public ulong Assets = 0;
        public ulong GraphAssets = 0;
        public ulong PlaylistAssets = 0;
        public ulong UrlSubmissions = 0;
        public ulong OtherAssets = 0;

        public ulong ExceptionLog = 0;
    }

    public sealed class EmojiConfig
    {
        public string Dot = "🅿";

        public string DisabledRepeat = "🅿";
        public string DisabledShuffle = "🅿";
        public string Paused = "🅿";
        public string DisabledPlay = "🅿";

        public string[] JoinEvent = { "🙋‍", "🙋‍" };
        public string Cuddle = "🅿";
        public string Kiss = "🅿";
        public string Slap = "🅿";
        public string Proud = "🅿";
        public string Hug = "🅿";

        public ulong WhiteXMark = 1005430134070841395;

        public ulong CheckboxTickedRedId = 970280327253725184;
        public ulong CheckboxUntickedRedId = 970280299466481745;

        public ulong CheckboxTickedBlueId = 970278964755038248;
        public ulong CheckboxUntickedBlueId = 970278964079767574;

        public ulong CheckboxTickedGreenId = 970280278138449920;
        public ulong CheckboxUntickedGreenId = 970280278025191454;

        public ulong PillOnId = 1027551252382494741;
        public ulong PillOffId = 1027551250818015322;

        public ulong QuestionMarkId = 1005464121472466984;

        public ulong ChannelId = 1005612865975238706;
        public ulong UserId = 1005612863051800746;
        public ulong VoiceStateId = 1005612864469487638;
        public ulong MessageId = 1005612861676077166;
        public ulong GuildId = 1005612867577458729;
        public ulong InviteId = 1005612860333899859;

        public ulong YouTubeId = 1011219477834252368;
        public ulong SoundCloudId = 1011219476001337444;
        public ulong AbuseIPDBId = 1022142812659126334;
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

        public GithubSecrets Github = new();
        public DatabaseSecrets Database = new();
        public LavalinkSecrets Lavalink = new();

        public sealed class GithubSecrets
        {
            public string Token = "";
            public DateTimeOffset TokenExperiation = new(0001, 01, 01, 15, 00, 00, TimeSpan.Zero);
            public string Username = "";
            public string Repository = "";
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
