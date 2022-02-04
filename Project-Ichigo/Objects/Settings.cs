namespace Project_Ichigo.Objects;

internal class Settings
{
    internal static Dictionary<ulong, ServerSettings> Servers = new();

    internal class ServerSettings
    {
        public ulong ServerId { get; set; } = 0;
        public ulong MemberRoleId { get; set; } = 0;
        public PhishingDetectionSettings PhishingDetectionSettings { get; set; } = new();
    }

    public class PhishingDetectionSettings
    {
        public PhishingPunishmentType PunishmentType { get; set; } = PhishingPunishmentType.BAN;
        public string CustomPunishmentReason { get; set; } = "";
        public TimeSpan CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
    }

    public enum PhishingPunishmentType
    {
        DELETE,
        TIMEOUT,
        KICK,
        BAN
    }
}
