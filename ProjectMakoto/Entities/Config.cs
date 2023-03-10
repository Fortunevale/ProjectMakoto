namespace ProjectMakoto.Entities;

public class Config
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

    public string SupportServerInvite = "";

    public ChannelsConfig Channels = new();
    public EmojiConfig Emojis = new();
    public LavalinkConfig Lavalink = new();
    public AccountIdsConfig Accounts = new();
    public SecretsConfig Secrets = new();
    public DontModifyConfig DontModify = new();

    public Dictionary<string, object> PluginData = new();

    public class ChannelsConfig
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

    public class LavalinkConfig
    {
        public bool UseAutoUpdater = false;
        public bool DownloadPreRelease = true;
        public string JarFolderPath = "";
    }

    public class EmojiConfig
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

    public class AccountIdsConfig
    {
        public ulong Disboard = 302050872383242240;
    }

    public class SecretsConfig
    {
        public string KawaiiRedToken = "";
        public string AbuseIpDbToken = "";
        public string LibreTranslateHost = "127.0.0.1";

        public GithubSecrets Github = new();
        public DatabaseSecrets Database = new();
        public LavalinkSecrets Lavalink = new();

        public class GithubSecrets
        {
            public string Token = "";
            public DateTimeOffset TokenExperiation = new(0001, 01, 01, 15, 00, 00, TimeSpan.Zero);
            public string Username = "";
            public string Repository = "";
            public string TokenLeakRepoOwner = "";
            public string TokenLeakRepo = "";
        }

        public class DatabaseSecrets
        {
            public string Host = "127.0.0.1";
            public int Port = 3306;
            public string Username = "";
            public string Password = "";

            public string MainDatabaseName = "";
            public string GuildDatabaseName = "";
        }

        public class LavalinkSecrets
        {
            public string Host = "127.0.0.1";
            public int Port = 2333;
            public string Password = "";
        }
    }

    public class DontModifyConfig
    {
        public string LastStartedVersion = "UNIDENTIFIED";
    }

    #region Legacy
    [Obsolete("Legacy")]
    [JsonProperty("UseLavalinkAutoUpdater")]
    public bool LegacyUseLavalinkAutoUpdater { set { Lavalink.UseAutoUpdater = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("LavalinkDownloadPreRelease")]
    public bool LegacyLavalinkDownloadPreRelease { set { Lavalink.DownloadPreRelease = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("LavalinkJarFolderPath")]
    public string LegacyLavalinkJarFolderPath { set { Lavalink.JarFolderPath = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("DotEmoji")]
    public string LegacyDotEmoji { set { Emojis.Dot = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("DisabledRepeatEmoji")]
    public string LegacyDisabledRepeatEmoji { set { Emojis.DisabledRepeat = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("DisabledShuffleEmoji")]
    public string LegacyDisabledShuffleEmoji { set { Emojis.DisabledShuffle = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("PausedEmoji")]
    public string LegacyPausedEmoji { set { Emojis.Paused = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("DisabledPlayEmoji")]
    public string LegacyDisabledPlayEmoji { set { Emojis.DisabledPlay = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("JoinEventsEmojis")]
    public string[] LegacyJoinEventsEmojis { set { Emojis.JoinEvent = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CuddleEmoji")]
    public string LegacyCuddleEmoji { set { Emojis.Cuddle = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("KissEmoji")]
    public string LegacyKissEmoji { set { Emojis.Kiss = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("SlapEmoji")]
    public string LegacySlapEmoji { set { Emojis.Slap = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("ProudEmoji")]
    public string LegacyProudEmoji { set { Emojis.Proud = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("HugEmoji")]
    public string LegacyHugEmoji { set { Emojis.Hug = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxTickedRedEmojiId")]
    public ulong LegacyCheckboxTickedRedEmojiId { set { Emojis.CheckboxTickedRedId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxUntickedRedEmojiId")]
    public ulong LegacyCheckboxUntickedRedEmojiId { set { Emojis.CheckboxUntickedRedId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxTickedBlueEmojiId")]
    public ulong LegacyCheckboxTickedBlueEmojiId { set { Emojis.CheckboxTickedBlueId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxUntickedBlueEmojiId")]
    public ulong LegacyCheckboxUntickedBlueEmojiId { set { Emojis.CheckboxUntickedBlueId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxTickedGreenEmojiId")]
    public ulong LegacyCheckboxTickedGreenEmojiId { set { Emojis.CheckboxTickedGreenId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("CheckboxUntickedGreenEmojiId")]
    public ulong LegacyCheckboxUntickedGreenEmojiId { set { Emojis.CheckboxUntickedGreenId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("QuestionMarkEmojiId")]
    public ulong LegacyQuestionMarkEmojiId { set { Emojis.QuestionMarkId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("ChannelEmojiId")]
    public ulong LegacyChannelEmojiId { set { Emojis.ChannelId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("UserEmojiId")]
    public ulong LegacyUserEmojiId { set { Emojis.UserId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("VoiceStateEmojiId")]
    public ulong LegacyVoiceStateEmojiId { set { Emojis.VoiceStateId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("MessageEmojiId")]
    public ulong LegacyMessageEmojiId { set { Emojis.MessageId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("GuildEmojiId")]
    public ulong LegacyGuildEmojiId { set { Emojis.GuildId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("InviteEmojiId")]
    public ulong LegacyInviteEmojiId { set { Emojis.InviteId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("YouTubeEmojiId")]
    public ulong LegacyYouTubeEmojiId { set { Emojis.YouTubeId = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("SoundCloudEmojiId")]
    public ulong LegacySoundCloudEmojiId { set { Emojis.SoundCloudId = value; } } 
    
    [Obsolete("Legacy")]
    [JsonProperty("DisboardAccountId")]
    public ulong LegacyDisboardAccountId { set { Accounts.Disboard = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("GlobalBanAnnouncementsChannelId")]
    public ulong LegacyGlobalBanAnnouncementsChannelId { set { Channels.GlobalBanAnnouncements = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("GithubLogChannelId")]
    public ulong LegacyGithubLogChannelId { set { Channels.GithubLog = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("NewsChannelId")]
    public ulong LegacyNewsChannelId { set { Channels.News = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("AssetsGuildId")]
    public ulong LegacyAssetsGuildId { set { Channels.Assets = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("GraphAssetsChannelId")]
    public ulong LegacyGraphAssetsChannelId { set { Channels.GraphAssets = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("PlaylistAssetsChannelId")]
    public ulong LegacyPlaylistAssetsChannelId { set { Channels.PlaylistAssets = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("UrlSubmissionsChannelId")]
    public ulong LegacyUrlSubmissionsChannelId { set { Channels.UrlSubmissions = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("OtherAssetsChannelId")]
    public ulong LegacyOtherAssetsChannelId { set { Channels.OtherAssets = value; } }

    [Obsolete("Legacy")]
    [JsonProperty("ExceptionLogChannelId")]
    public ulong LegacyExceptionLogChannelId { set { Channels.ExceptionLog = value; } }
    #endregion
}
