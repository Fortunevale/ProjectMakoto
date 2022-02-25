namespace Project_Ichigo.Objects.Database;

internal class DatabaseColumnLists
{
    internal readonly static List<string> scam_urls = new() 
    {
        "url", 
        "origin", 
        "submitter", 
    };
    
    internal readonly static List<string> user_submission_bans = new() 
    {
        "id",
        "reason",
        "moderator",
    };
    
    internal readonly static List<string> guild_submission_bans = new() 
    {
        "id",
        "reason",
        "moderator",
    };
    
    internal readonly static List<string> active_url_submissions = new() 
    {
        "messageid",
        "url",
        "submitter",
        "guild",
    };
    
    internal readonly static List<string> users = new() 
    {
        "userid",
        "scoresaber_id",
        "afk_reason",
        "afk_since",
        "submission_accepted_tos",
        "submission_accepted_submissions",
        "submission_last_datetime",
    };
    
    internal readonly static List<string> guilds = new() 
    { 
        "serverid", 
        "bump_enabled", 
        "bump_role", 
        "bump_channel", 
        "bump_last_reminder", 
        "bump_last_time", 
        "bump_last_user", 
        "bump_message", 
        "bump_persistent_msg", 
        "phishing_detect", 
        "phishing_type", 
        "phishing_reason", 
        "phishing_time", 
    };
}
