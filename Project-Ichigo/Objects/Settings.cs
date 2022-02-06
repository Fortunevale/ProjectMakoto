namespace Project_Ichigo.Objects;

internal class Settings
{
    internal Dictionary<ulong, ServerSettings> Servers = new();

    internal class ServerSettings
    {
        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
    }

    public class PhishingDetectionSettings
    {
        private bool _DetectPhishing { get; set; } = true;
        public bool DetectPhishing { get => _DetectPhishing; set { _DetectPhishing = value; _ = Bot._databaseHelper.SyncGuilds(); } }


        private PhishingPunishmentType _PunishmentType { get; set; } = PhishingPunishmentType.BAN;
        public PhishingPunishmentType PunishmentType { get => _PunishmentType; set { _PunishmentType = value; _ = Bot._databaseHelper.SyncGuilds(); } }


        private string _CustomPunishmentReason { get; set; } = "Reason: %R (%u)";
        public string CustomPunishmentReason { get => _CustomPunishmentReason; set { _CustomPunishmentReason = value; _ = Bot._databaseHelper.SyncGuilds(); } }


        private TimeSpan _CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
        public TimeSpan CustomPunishmentLength { get => _CustomPunishmentLength; set { _CustomPunishmentLength = value; _ = Bot._databaseHelper.SyncGuilds(); } }
    }

    public enum PhishingPunishmentType
    {
        DELETE,
        TIMEOUT,
        KICK,
        BAN
    }
}
