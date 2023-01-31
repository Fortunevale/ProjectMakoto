namespace ProjectMakoto.Entities;

public class Guild
{
    public Guild(ulong serverId, Bot bot)
    {
        _bot = bot;

        ServerId = serverId;

        TokenLeakDetection = new(this);
        PhishingDetection = new(this);
        BumpReminder = new(this);
        Join = new(this);
        Experience = new(this);
        Crosspost = new(this);
        ActionLog = new(this);
        InVoiceTextPrivacy = new(this);
        InviteTracker = new(this);
        InviteNotes = new(this);
        NameNormalizer = new(this);
        EmbedMessage = new(this);
        MusicModule = new(this);
        Polls = new(this, bot);
        VcCreator = new(this, bot);
    }

    private Bot _bot { get; set; }
    public ulong ServerId { get; set; }

    public TokenLeakDetectionSettings TokenLeakDetection { get; set; }
    public PhishingDetectionSettings PhishingDetection { get; set; }
    public BumpReminderSettings BumpReminder { get; set; }
    public JoinSettings Join { get; set; }
    public ExperienceSettings Experience { get; set; }
    public CrosspostSettings Crosspost { get; set; }
    public ActionLogSettings ActionLog { get; set; }
    public InVoiceTextPrivacySettings InVoiceTextPrivacy { get; set; }
    public InviteTrackerSettings InviteTracker { get; set; }
    public InviteNotesSettings InviteNotes { get; set; }
    public NameNormalizerSettings NameNormalizer { get; set; }
    public EmbedMessageSettings EmbedMessage { get; set; }
    public PollSettings Polls { get; set; }
    public VcCreatorSettings VcCreator { get; set; }

    public List<ulong> AutoUnarchiveThreads { get; set; } = new();
    public List<LevelRewardEntry> LevelRewards { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoleEntry>> ReactionRoles { get; set; } = new();

    public Dictionary<ulong, Member> Members { get; set; } = new();

    public Lavalink MusicModule { get; set; }

    private string _Prefix { get; set; } = "";
    public string Prefix
    {
        get => _Prefix.IsNullOrWhiteSpace() ? _bot.Prefix : _Prefix; set
        {
            _Prefix = value.IsNullOrWhiteSpace() ? _bot.Prefix : value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", ServerId, "prefix", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
    
    private bool _PrefixDisabled { get; set; } = false;
    public bool PrefixDisabled
    {
        get => _PrefixDisabled; set
        {
            _PrefixDisabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", ServerId, "prefix_disabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
