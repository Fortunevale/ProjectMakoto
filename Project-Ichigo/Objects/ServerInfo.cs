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
        public Dictionary<ulong, Members> Members { get; set; } = new();
    }
}
