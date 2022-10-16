namespace ProjectIchigo.Entities;

public class Guild
{
    public Guild(ulong serverId, Bot bot)
    {
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
    }

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

    public List<ulong> AutoUnarchiveThreads { get; set; } = new();
    public List<LevelRewardEntry> LevelRewards { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoleEntry>> ReactionRoles { get; set; } = new();

    public Dictionary<ulong, Member> Members { get; set; } = new();

    public Lavalink MusicModule { get; set; }
}
