namespace ProjectIchigo.Entities;

public class Guild
{
    public Guild(ulong serverId)
    {
        ServerId = serverId;

        TokenLeakDetectionSettings = new(this);
        PhishingDetectionSettings = new(this);
        BumpReminderSettings = new(this);
        JoinSettings = new(this);
        ExperienceSettings = new(this);
        CrosspostSettings = new(this);
        ActionLogSettings = new(this);
        InVoiceTextPrivacySettings = new(this);
        InviteTrackerSettings = new(this);
        NameNormalizerSettings = new(this);
        EmbedMessageSettings = new(this);
        Lavalink = new(this);

        ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated;
    }

    public ulong ServerId { get; set; }

    public TokenLeakDetectionSettings TokenLeakDetectionSettings { get; set; }
    public PhishingDetectionSettings PhishingDetectionSettings { get; set; }
    public BumpReminderSettings BumpReminderSettings { get; set; }
    public JoinSettings JoinSettings { get; set; }
    public ExperienceSettings ExperienceSettings { get; set; }
    public CrosspostSettings CrosspostSettings { get; set; }
    public ActionLogSettings ActionLogSettings { get; set; }
    public InVoiceTextPrivacySettings InVoiceTextPrivacySettings { get; set; }
    public InviteTrackerSettings InviteTrackerSettings { get; set; }
    public NameNormalizerSettings NameNormalizerSettings { get; set; }
    public EmbedMessageSettings EmbedMessageSettings { get; set; }

    public Lavalink Lavalink { get; set; }

    private ObservableList<ulong> _ProcessedAuditLogs { get; set; } = new();
    public ObservableList<ulong> ProcessedAuditLogs { get => _ProcessedAuditLogs; set { _ProcessedAuditLogs = value; _ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated; } }

    public List<ulong> AutoUnarchiveThreads { get; set; } = new();

    public List<LevelRewardEntry> LevelRewards { get; set; } = new();
    public Dictionary<ulong, Member> Members { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoleEntry>> ReactionRoles { get; set; } = new();

    private void AuditLogCollectionUpdated(object sender, object e)
    {
        while (ProcessedAuditLogs.Count > 50)
        {
            _logger.LogDebug($"Removing {ProcessedAuditLogs[0]}");
            ProcessedAuditLogs.RemoveAt(0);
        }
    }
}
