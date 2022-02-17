namespace Project_Ichigo.Objects;

internal class ServerInfo
{
    internal Dictionary<ulong, ServerSettings> Servers = new();

    internal class ServerSettings
    {
        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
        public BumpReminderSettings BumpReminderSettings { get; set; } = new();
    }

    public class BumpReminderSettings
    {
        private bool _Enabled { get; set; } = false;
        public bool Enabled { get => _Enabled; set { _Enabled = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private ulong _RoleId { get; set; } = 0;
        public ulong RoleId { get => _RoleId; set { _RoleId = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private ulong _ChannelId { get; set; } = 0;
        public ulong ChannelId { get => _ChannelId; set { _ChannelId = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private ulong _MessageId { get; set; } = 0;
        public ulong MessageId { get => _MessageId; set { _MessageId = value; _ = Bot._databaseHelper.SyncDatabase(); } }
        

        private ulong _PersistentMessageId { get; set; } = 0;
        public ulong PersistentMessageId { get => _PersistentMessageId; set { _PersistentMessageId = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private ulong _LastUserId { get; set; } = 0;
        public ulong LastUserId { get => _LastUserId; set { _LastUserId = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private DateTime _LastBump { get; set; } = DateTime.MinValue;
        public DateTime LastBump { get => _LastBump; set { _LastBump = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private DateTime _LastReminder { get; set; } = DateTime.MinValue;
        public DateTime LastReminder { get => _LastReminder; set { _LastReminder = value; _ = Bot._databaseHelper.SyncDatabase(); } }
    }

    public class PhishingDetectionSettings
    {
        private bool _DetectPhishing { get; set; } = true;
        public bool DetectPhishing { get => _DetectPhishing; set { _DetectPhishing = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private PhishingPunishmentType _PunishmentType { get; set; } = PhishingPunishmentType.BAN;
        public PhishingPunishmentType PunishmentType { get => _PunishmentType; set { _PunishmentType = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private string _CustomPunishmentReason { get; set; } = "%R";
        public string CustomPunishmentReason { get => _CustomPunishmentReason; set { _CustomPunishmentReason = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private TimeSpan _CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
        public TimeSpan CustomPunishmentLength { get => _CustomPunishmentLength; set { _CustomPunishmentLength = value; _ = Bot._databaseHelper.SyncDatabase(); } }
    }

    public enum PhishingPunishmentType
    {
        DELETE,
        TIMEOUT,
        KICK,
        BAN
    }
}
