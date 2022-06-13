namespace ProjectIchigo.Entities.Legacy;

internal class UserCache
{
    internal static Dictionary<ulong, UserCacheObjects> Users = new();

    internal class UserCacheObjects
    {
        public CachedInvitedBy InvitedBy { get; set; } = new CachedInvitedBy();

        public string SavedNickname { get; set; } = "";
        public string AvatarAssetUrl { get; set; } = "";
        public string LastAvatarHash { get; set; } = "";

        public DateTime FirstJoined { get; set; } = new DateTime();

        public bool IsOnServer { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public DateTime DateLeft { get; set; } = new DateTime();

        public List<Warnings.WarningInfo> Warnings { get; set; } = new();
        public List<Warnings.ExpiredWarningInfo> ExpiredWarnings { get; set; } = new();

        public bool SentFirstMessage { get; set; } = false;

        public List<SavedRole> SavedRoles { get; set; } = new();
        public Dictionary<Guid, PunishmentHistory> PunishmentHistory { get; set; } = new();

        public ulong LastTextChat { get; set; } = 0;
        public DateTime LastTextChatAt { get; set; } = new DateTime();

        public ulong CurrentVoiceChat { get; set; } = 0;
        public DateTime JoinedVoiceChatAt { get; set; } = new DateTime();
        public DateTime LastVoiceChatRepuationAt { get; set; } = new DateTime();

        public bool OptOutDirectMessageLevelRewards { get; set; } = false;

        public int ExperienceLevel { get; set; } = 1;
        public float Experience { get; set; } = 150;
        public Dictionary<string, Experience.MultiplierInfo> ExperienceMultiplier { get; set; } = new();

        public DateTime MessageReputationCooldown { get; set; } = new DateTime();

        public Dictionary<string, DateTime> CurrentActivities { get; set; } = new();

        public Dictionary<string, Int64> StoredActivities { get; set; } = new();
    }

    internal class SavedRole
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
    }

    internal class CachedInvitedBy
    {
        public ulong Id { get; set; }
        public string Code { get; set; }
        public DateTime Timeout { get; set; }
    }

    internal class PunishmentHistory
    {
        public PunishmentActions PunishmentType { get; set; }
        public string Reason { get; set; }
        public ulong Moderator { get; set; }
    }

    internal enum PunishmentActions
    {
        BAN,
        KICK,
        MUTE,
        WARN
    }
}