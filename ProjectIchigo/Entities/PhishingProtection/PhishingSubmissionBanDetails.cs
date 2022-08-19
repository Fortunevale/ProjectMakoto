namespace ProjectIchigo;

internal class PhishingSubmissionBanDetails
{
    private string _Reason { get; set; }
    public string Reason { get => _Reason; set { _Reason = value; _ = Bot.DatabaseClient.FullSyncDatabase(); } }


    private ulong _Moderator { get; set; }
    public ulong Moderator { get => _Moderator; set { _Moderator = value; _ = Bot.DatabaseClient.FullSyncDatabase(); } }
}
