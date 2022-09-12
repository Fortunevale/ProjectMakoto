namespace ProjectIchigo.Entities.Database;

public class DatabaseGuildSettings
{
    public ulong serverid { get; set; }
    public string levelrewards { get; set; }
    public string auditlogcache { get; set; }
    public string reactionroles { get; set; }
    public bool crosspostexcludebots { get; set; }
    public int crosspostdelay { get; set; }
    public string crosspostchannels { get; set; }
    public bool reapplyroles { get; set; }
    public bool reapplynickname { get; set; }
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
    public long bump_last_time { get; set; }
    public long bump_last_reminder { get; set; }
    public int bump_missed { get; set; }
    public bool tokens_detect { get; set; }
    public bool phishing_detect { get; set; }
    public bool phishing_warnonredirect { get; set; }
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
    public bool actionlog_log_voice_state { get; set; }
    public bool actionlog_log_invites_modified { get; set; }
    public bool vc_privacy_perms { get; set; }
    public bool vc_privacy_clear { get; set; }
    public bool invitetracker_enabled { get; set; }
    public string invitetracker_cache { get; set; }
    public string autounarchivelist { get; set; }
    public bool normalizenames { get; set; }
    public bool embed_messages { get; set; }
    public bool embed_github { get; set; }
    public ulong lavalink_channel { get; set; }
    public string lavalink_currentvideo { get; set; }
    public long lavalink_currentposition { get; set; }
    public bool lavalink_paused { get; set; }
    public bool lavalink_shuffle { get; set; }
    public bool lavalink_repeat { get; set; }
    public string lavalink_queue { get; set; }
    public string crosspost_ratelimits { get; set; }
}
