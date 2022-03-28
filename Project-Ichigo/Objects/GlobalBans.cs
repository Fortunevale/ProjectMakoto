namespace Project_Ichigo.Objects;

internal class GlobalBans
{
    public Dictionary<ulong, BanInfo> Users = new();

    public class BanInfo
    {
        private string _Reason { get; set; }
        public string Reason { get => _Reason; set { _Reason = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


        private ulong _Moderator { get; set; }
        public ulong Moderator { get => _Moderator; set { _Moderator = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
    }
}
