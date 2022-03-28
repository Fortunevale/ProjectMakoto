namespace Project_Ichigo.Objects;

public class PhishingDetectionSettings
{
    private bool _DetectPhishing { get; set; } = true;
    public bool DetectPhishing { get => _DetectPhishing; set { _DetectPhishing = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private PhishingPunishmentType _PunishmentType { get; set; } = PhishingPunishmentType.BAN;
    public PhishingPunishmentType PunishmentType { get => _PunishmentType; set { _PunishmentType = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private string _CustomPunishmentReason { get; set; } = "%R";
    public string CustomPunishmentReason { get => _CustomPunishmentReason; set { _CustomPunishmentReason = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private TimeSpan _CustomPunishmentLength { get; set; } = TimeSpan.FromDays(14);
    public TimeSpan CustomPunishmentLength { get => _CustomPunishmentLength; set { _CustomPunishmentLength = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}

public enum PhishingPunishmentType
{
    DELETE,
    TIMEOUT,
    KICK,
    BAN
}
