namespace Project_Ichigo.Objects.Database;

public class DatabaseServerSettings
{
    public ulong serverid { get; set; }
    public string levelrewards { get; set; }
    public string auditlogcache { get; set; }
    public ulong auto_assign_role_id { get; set; }
    public ulong joinlog_channel_id { get; set; }
    public bool autoban_global_ban { get; set; }
    public bool experience_use { get; set; }
    public bool experience_boost_bumpreminder { get; set; }
    public bool bump_enabled { get; set; }
    public ulong bump_role { get; set; }
    public ulong bump_channel { get; set; }
    public ulong bump_message { get; set; }
    public ulong bump_persistent_msg { get; set; }
    public ulong bump_last_user { get; set; }
    public ulong bump_last_time { get; set; }
    public ulong bump_last_reminder { get; set; }
    public bool phishing_detect { get; set; }
    public int phishing_type { get; set; }
    public string phishing_reason { get; set; }
    public long phishing_time { get; set; }

    public ulong actionlog_channel { get; set; }
    public bool actionlog_attempt_further_detail { get; set; }
    public bool actionlog_log_members_modified { get; set; }
    public bool actionlog_log_member_modified { get; set; }
    public bool actionlog_log_memberprofile_modified { get; set; }
    public bool actionlog_log_message_deleted { get; set; }
    public bool actionlog_log_message_updated { get; set; }
    public bool actionlog_log_roles_modified { get; set; }
    public bool actionlog_log_banlist_modified { get; set; }
    public bool actionlog_log_guild_modified { get; set; }
    public bool actionlog_log_channels_modified { get; set; }
    public bool actionlog_log_invites_modified { get; set; }
}
