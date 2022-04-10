namespace Project_Ichigo.Objects;

internal class ServerInfo
{
    internal Dictionary<ulong, ServerSettings> Servers = new();

    internal class ServerSettings
    {
        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
        public BumpReminderSettings BumpReminderSettings { get; set; } = new();
        public JoinSettings JoinSettings { get; set; } = new();
        public ExperienceSettings ExperienceSettings { get; set; } = new();
        public List<LevelRewards> LevelRewards { get; set; } = new();
        public Dictionary<ulong, Members> Members { get; set; } = new();
        public ActionLogSettings ActionLogSettings { get; set; } = new();
        public ObservableCollection<ulong> ProcessedAuditLogs { get; set; } = new();
        public ObservableCollection<ulong> CrosspostChannels { get; set; } = new();
    }
}
