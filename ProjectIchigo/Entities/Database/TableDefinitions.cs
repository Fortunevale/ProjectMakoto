using ProjectIchigo.Entities.Database.ColumnAttributes;
using ProjectIchigo.Entities.Database.ColumnTypes;

namespace ProjectIchigo.Entities.Database;

public class TableDefinitions
{
    public class writetester
    {
        [Primary][MaxValue(1)] public TinyInt aaa { get; set; }
    }

    public class scam_urls
    {
        [Primary][Collation("utf8_unicode_ci")][MaxValue(500)]
        public VarChar url { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text origin { get; set; }

        public BigInt submitter { get; set; }
    }

    public class objected_users
    {
        [Primary]
        public BigInt id { get; set; }
    }

    public class globalbans
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }

    public class banned_users
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }
    
    public class banned_guilds
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }

    public class globalnotes
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText notes { get; set; }
    }

    public class submission_user_bans
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }
    }

    public class submission_guild_bans
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }
    }

    public class active_url_submissions
    {
        [Primary]
        public BigInt messageid { get; set; }

        [Collation("utf8_unicode_ci")][MaxValue(500)]
        public VarChar url { get; set; }

        public BigInt submitter { get; set; }
        public BigInt guild { get; set; }
    }

    public class users
    {
        [Primary]
        public BigInt userid { get; set; }

        public BigInt scoresaber_id { get; set; }

        public BigInt afk_since { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text afk_reason { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text afk_pings { get; set; }

        public BigInt afk_pingamount { get; set; }

        [MaxValue(1)]
        public TinyInt experience_directmessageoptout { get; set; }

        public TinyInt submission_accepted_tos { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText submission_accepted_submissions { get; set; }

        public BigInt submission_last_datetime { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText playlists { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText reminders { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text last_google_source { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text last_google_target { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text last_libretranslate_source { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text last_libretranslate_target { get; set; }
        
        [Collation("utf8_unicode_ci")]
        public Text locale { get; set; }
    }

    public class guilds
    {
        [Primary]
        public BigInt serverid { get; set; }

        public BigInt auto_assign_role_id { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text levelrewards { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text auditlogcache { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text reactionroles { get; set; }

        [MaxValue(1)]
        public TinyInt crosspostexcludebots { get; set; }

        [Default("10")]
        public Int crosspostdelay { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text crosspostchannels { get; set; }

        [MaxValue(1)]
        public TinyInt normalizenames { get; set; }

        [MaxValue(1)]
        public TinyInt reapplyroles { get; set; }

        [MaxValue(1)]
        public TinyInt reapplynickname { get; set; }

        public BigInt joinlog_channel_id { get; set; }

        [MaxValue(1)]
        public TinyInt experience_use { get; set; }

        [MaxValue(1)]
        public TinyInt experience_boost_bumpreminder { get; set; }

        [MaxValue(1)]
        public TinyInt autoban_global_ban { get; set; }

        [MaxValue(1)]
        public TinyInt bump_enabled { get; set; }

        public BigInt bump_role { get; set; }

        public BigInt bump_channel { get; set; }

        public BigInt bump_last_reminder { get; set; }

        public BigInt bump_last_time { get; set; }

        public BigInt bump_last_user { get; set; }

        public BigInt bump_message { get; set; }

        public BigInt bump_persistent_msg { get; set; }

        public Int bump_missed { get; set; }

        [MaxValue(1)]
        public TinyInt tokens_detect { get; set; }

        [MaxValue(1)]
        public TinyInt phishing_detect { get; set; }

        [MaxValue(1)]
        public TinyInt phishing_warnonredirect { get; set; }

        [MaxValue(1)]
        public TinyInt phishing_abuseipdb { get; set; }

        public Int phishing_type { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text phishing_reason { get; set; }

        public BigInt phishing_time { get; set; }

        public BigInt actionlog_channel { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_attempt_further_detail { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_members_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_member_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_memberprofile_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_message_deleted { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_message_updated { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_roles_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_banlist_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_guild_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_channels_modified { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_voice_state { get; set; }

        [MaxValue(1)]
        public TinyInt actionlog_log_invites_modified { get; set; }

        [MaxValue(1)]
        public TinyInt vc_privacy_perms { get; set; }

        [MaxValue(1)]
        public TinyInt vc_privacy_clear { get; set; }

        [MaxValue(1)]
        public TinyInt invitetracker_enabled { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text invitetracker_cache { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text autounarchivelist { get; set; }

        [MaxValue(1)]
        public TinyInt embed_messages { get; set; }

        [MaxValue(1)]
        public TinyInt embed_github { get; set; }

        public BigInt lavalink_channel { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text lavalink_currentvideo { get; set; }

        public BigInt lavalink_currentposition { get; set; }

        [MaxValue(1)]
        public TinyInt lavalink_paused { get; set; }

        [MaxValue(1)]
        public TinyInt lavalink_shuffle { get; set; }

        [MaxValue(1)]
        public TinyInt lavalink_repeat { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText lavalink_queue { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText polls { get; set; }

        public BigInt vccreator_channelid { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText vccreator_channellist { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText invitenotes { get; set; }

        [Collation("utf8_unicode_ci")]
        public LongText crosspost_ratelimits { get; set; }
    }

    public class guild_users
    {
        [Primary]
        public BigInt userid { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text saved_nickname { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text roles { get; set; }

        public BigInt invite_user { get; set; }

        [Collation("utf8_unicode_ci")]
        public Text invite_code { get; set; }

        public BigInt first_join { get; set; }

        public BigInt last_leave { get; set; }

        public BigInt experience_last_message { get; set; }

        public BigInt experience { get; set; }

        public BigInt experience_level { get; set; }
    }

    public readonly static IReadOnlyList<Type> TableList = new List<Type>()
    {
        typeof(writetester),

        typeof(users),
        typeof(guilds),

        typeof(scam_urls),

        typeof(banned_users),
        typeof(banned_guilds),

        typeof(globalbans),
        typeof(globalnotes),

        typeof(submission_user_bans),
        typeof(submission_guild_bans),
        typeof(active_url_submissions),
        
        typeof(objected_users),
    };
}
