namespace Project_Ichigo.PhishingProtection;

internal class SubmissionBans
{
    public Dictionary<string, BanInfo> BannedUsers = new();
    public Dictionary<string, BanInfo> BannedGuilds = new();

    public class BanInfo
    {
        public string Reason { get; set; }
        public string Moderator { get; set; }
    }
}
