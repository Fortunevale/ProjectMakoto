﻿namespace Project_Ichigo.Objects;

internal class Guilds
{
    internal Dictionary<ulong, ServerSettings> List = new();

    internal class ServerSettings
    {
        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
        public BumpReminderSettings BumpReminderSettings { get; set; } = new();
        public JoinSettings JoinSettings { get; set; } = new();
        public ExperienceSettings ExperienceSettings { get; set; } = new();
        public List<LevelReward> LevelRewards { get; set; } = new();
        public Dictionary<ulong, Member> Members { get; set; } = new();
        public List<KeyValuePair<ulong, ReactionRoles>> ReactionRoles { get; set; } = new();
        public ActionLogSettings ActionLogSettings { get; set; } = new();
        public ObservableCollection<ulong> ProcessedAuditLogs { get; set; } = new();
        public ObservableCollection<ulong> CrosspostChannels { get; set; } = new();
    }
}
