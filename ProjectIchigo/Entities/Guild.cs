namespace ProjectIchigo.Entities;

public class Guild
{
    public Guild(ulong serverId)
    {
        ServerId = serverId;

        PhishingDetectionSettings = new(this);
        BumpReminderSettings = new(this);
        JoinSettings = new(this);
        ExperienceSettings = new(this);
        CrosspostSettings = new(this);
        ActionLogSettings = new(this);
        InVoiceTextPrivacySettings = new(this);
        InviteTrackerSettings = new(this);
        NameNormalizerSettings = new(this);

        CrosspostSettings.CrosspostChannels.CollectionChanged += CrosspostSettings.CrosspostCollectionUpdated();
        AutoUnarchiveThreads.CollectionChanged += UnarchiveThreadsUpdated();
        ProcessedAuditLogs.CollectionChanged += AuditLogCollectionUpdated();
    }

    ~Guild()
    {
        CrosspostSettings.CrosspostChannels.CollectionChanged -= CrosspostSettings.CrosspostCollectionUpdated();
        ProcessedAuditLogs.CollectionChanged -= AuditLogCollectionUpdated();
        AutoUnarchiveThreads.CollectionChanged -= UnarchiveThreadsUpdated();
    }

    public ulong ServerId { get; set; }

    public PhishingDetectionSettings PhishingDetectionSettings { get; set; }
    public BumpReminderSettings BumpReminderSettings { get; set; }
    public JoinSettings JoinSettings { get; set; }
    public ExperienceSettings ExperienceSettings { get; set; }
    public CrosspostSettings CrosspostSettings { get; set; }
    public ActionLogSettings ActionLogSettings { get; set; }
    public InVoiceTextPrivacySettings InVoiceTextPrivacySettings { get; set; }
    public InviteTrackerSettings InviteTrackerSettings { get; set; }
    public NameNormalizerSettings NameNormalizerSettings { get; set; }

    public Lavalink Lavalink { get; set; } = new();

    public ObservableCollection<ulong> ProcessedAuditLogs { get; set; } = new();
    public ObservableCollection<ulong> AutoUnarchiveThreads { get; set; } = new();
    public List<LevelReward> LevelRewards { get; set; } = new();
    public Dictionary<ulong, Member> Members { get; set; } = new();
    public List<KeyValuePair<ulong, ReactionRoles>> ReactionRoles { get; set; } = new();

    private NotifyCollectionChangedEventHandler AuditLogCollectionUpdated()
    {
        return (s, e) =>
        {
            if (ProcessedAuditLogs.Count > 50)
                ProcessedAuditLogs.Remove(ProcessedAuditLogs[0]);

            _ = Bot.DatabaseClient.FullSyncDatabase();
        };
    }

    private NotifyCollectionChangedEventHandler UnarchiveThreadsUpdated()
    {
        return (s, e) =>
        {
            _ = Bot.DatabaseClient.FullSyncDatabase();
        };
    }
}
