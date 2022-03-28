namespace Project_Ichigo.PhishingProtection;

internal class SubmissionBans
{
    public Dictionary<ulong, BanInfo> BannedUsers = new();
    public Dictionary<ulong, BanInfo> BannedGuilds = new();

    public class BanInfo
    {
        private string _Reason { get; set; }
        public string Reason { get => _Reason; set { _Reason = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


        private ulong _Moderator { get; set; }
        public ulong Moderator { get => _Moderator; set { _Moderator = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
    }
}
