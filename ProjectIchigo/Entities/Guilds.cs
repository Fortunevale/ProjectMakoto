namespace ProjectIchigo.Entities;

internal class Guilds
{
    internal Dictionary<ulong, ServerSettings> List = new();

    internal class ServerSettings
    {
        internal ServerSettings()
        {
            CrosspostSettings.CrosspostChannels.CollectionChanged += CrosspostSettings.CrosspostCollectionUpdated();
            AutoUnarchiveThreads.CollectionChanged += UnarchiveThreadsUpdated();
            ProcessedAuditLogs.CollectionChanged += AuditLogCollectionUpdated();
        }

        ~ServerSettings()
        {
            CrosspostSettings.CrosspostChannels.CollectionChanged -= CrosspostSettings.CrosspostCollectionUpdated();
            ProcessedAuditLogs.CollectionChanged -= AuditLogCollectionUpdated();
            AutoUnarchiveThreads.CollectionChanged -= UnarchiveThreadsUpdated();
        }

        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
        public BumpReminderSettings BumpReminderSettings { get; set; } = new();
        public JoinSettings JoinSettings { get; set; } = new();
        public ExperienceSettings ExperienceSettings { get; set; } = new();
        public List<LevelReward> LevelRewards { get; set; } = new();
        public Dictionary<ulong, Member> Members { get; set; } = new();
        public List<KeyValuePair<ulong, ReactionRoles>> ReactionRoles { get; set; } = new();
        public CrosspostSettings CrosspostSettings { get; set; } = new();
        public ActionLogSettings ActionLogSettings { get; set; } = new();
        public InVoiceTextPrivacySettings InVoiceTextPrivacySettings { get; set; } = new();
        public InviteTrackerSettings InviteTrackerSettings { get; set; } = new();
        public ObservableCollection<ulong> ProcessedAuditLogs { get; set; } = new();
        public ObservableCollection<ulong> AutoUnarchiveThreads { get; set; } = new();
        public Lavalink Lavalink { get; set; } = new();
        public NameNormalizerSettings NameNormalizer { get; set; } = new();

        internal NotifyCollectionChangedEventHandler AuditLogCollectionUpdated()
        {
            return (s, e) =>
            {
                if (ProcessedAuditLogs.Count > 50)
                    ProcessedAuditLogs.Remove(ProcessedAuditLogs[0]);

                _ = Bot.DatabaseClient.SyncDatabase();
            };
        }

        internal NotifyCollectionChangedEventHandler UnarchiveThreadsUpdated()
        {
            return (s, e) =>
            {
                _ = Bot.DatabaseClient.SyncDatabase();
            };
        }
    }
}
