namespace Project_Ichigo.Entities.Legacy;

internal class Warnings
{
    public class WarningInfo
    {
        public int Severity { get; set; }
        public string Reason { get; set; }
        public string ModeratorName { get; set; }
        public int ModeratorDiscriminator { get; set; }
        public ulong ModeratorId { get; set; }
        public ulong MessageId { get; set; }
        public string WarningUUID { get; set; }
        public DateTime WarningTime { get; set; }
    }

    public class ExpiredWarningList
    {
        public List<ExpiredWarningInfo> Warnungen = new();
    }

    public class ExpiredWarningInfo
    {
        public int Severity { get; set; }
        public string Reason { get; set; }
        public string ModeratorName { get; set; }
        public int ModeratorDiscriminator { get; set; }
        public ulong ModeratorId { get; set; }
        public string WarningUUID { get; set; }
        public DateTime WarningTime { get; set; }
    }
}
