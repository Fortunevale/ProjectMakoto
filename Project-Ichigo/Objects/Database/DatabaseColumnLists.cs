namespace Project_Ichigo.Objects.Database;

internal class DatabaseColumnLists
{
    internal readonly static List<Column> writetester = new()
    {
        new Column("aaa", "tinyint(1)", primary: true)
    };

    internal readonly static List<Column> scam_urls = new()
    {
        new Column("url", "varchar(500)", "utf8mb4_0900_ai_ci", "", false, true),
        new Column("origin", "text", "utf8mb4_0900_ai_ci", "", false, false),
        new Column("submitter", "bigint"),
    };

    internal readonly static List<Column> globalbans = new()
    {
        new Column("id", "bigint", primary: true),
        new Column("reason", "text", "utf8mb4_0900_ai_ci"),
        new Column("moderator", "bigint"),
    };

    internal readonly static List<Column> user_submission_bans = new()
    {
        new Column("id", "bigint", primary: true),
        new Column("reason", "text", "utf8mb4_0900_ai_ci"),
        new Column("moderator", "bigint"),
    };

    internal readonly static List<Column> guild_submission_bans = new()
    {
        new Column("id", "bigint", primary: true),
        new Column("reason", "text", "utf8mb4_0900_ai_ci"),
        new Column("moderator", "bigint"),
    };

    internal readonly static List<Column> active_url_submissions = new()
    {
        new Column("messageid", "bigint", primary: true),
        new Column("url", "text", "utf8mb4_0900_ai_ci"),
        new Column("submitter", "bigint"),
        new Column("guild", "bigint"),
    };

    internal readonly static List<Column> users = new()
    {
        new Column("userid", "bigint", primary: true),
        new Column("scoresaber_id", "bigint"),
        new Column("afk_since", "bigint"),
        new Column("afk_reason", "text", "utf8mb4_0900_ai_ci"),
        new Column("afk_pings", "text", "utf8mb4_0900_ai_ci"),
        new Column("afk_pingamount", "bigint"),
        new Column("experience_directmessageoptout", "tinyint(1)"),
        new Column("submission_accepted_tos", "tinyint(1)"),
        new Column("submission_accepted_submissions", "text", "utf8mb4_0900_ai_ci"),
        new Column("submission_last_datetime", "datetime"),
    };

    internal readonly static List<Column> guilds = new()
    {
        new Column("serverid", "bigint", primary: true),
        new Column("auto_assign_role_id", "bigint"),
        new Column("levelrewards", "text", "utf8mb4_0900_ai_ci"),
        new Column("auditlogcache", "text", "utf8mb4_0900_ai_ci"),
        new Column("reactionroles", "text", "utf8mb4_0900_ai_ci"),
        new Column("crosspostchannels", "text", "utf8mb4_0900_ai_ci"),
        new Column("reapplyroles", "tinyint(1)"),
        new Column("reapplynickname", "tinyint(1)"),
        new Column("joinlog_channel_id", "bigint"),
        new Column("experience_use", "tinyint(1)"),
        new Column("experience_boost_bumpreminder", "tinyint(1)"),
        new Column("autoban_global_ban", "tinyint(1)"),
        new Column("bump_enabled", "tinyint(1)"),
        new Column("bump_role", "bigint"),
        new Column("bump_channel", "bigint"),
        new Column("bump_last_reminder", "bigint"),
        new Column("bump_last_time", "bigint"),
        new Column("bump_last_user", "bigint"),
        new Column("bump_message", "bigint"),
        new Column("bump_persistent_msg", "bigint"),
        new Column("phishing_detect", "tinyint(1)"),
        new Column("phishing_type", "int"),
        new Column("phishing_reason", "text", "utf8mb4_0900_ai_ci"),
        new Column("phishing_time", "bigint"),
        new Column("actionlog_channel", "bigint"),
        new Column("actionlog_attempt_further_detail", "tinyint(1)"),
        new Column("actionlog_log_members_modified", "tinyint(1)"),
        new Column("actionlog_log_member_modified", "tinyint(1)"),
        new Column("actionlog_log_memberprofile_modified", "tinyint(1)"),
        new Column("actionlog_log_message_deleted", "tinyint(1)"),
        new Column("actionlog_log_message_updated", "tinyint(1)"),
        new Column("actionlog_log_roles_modified", "tinyint(1)"),
        new Column("actionlog_log_banlist_modified", "tinyint(1)"),
        new Column("actionlog_log_guild_modified", "tinyint(1)"),
        new Column("actionlog_log_channels_modified", "tinyint(1)"),
        new Column("actionlog_log_voice_state", "tinyint(1)"),
        new Column("actionlog_log_invites_modified", "tinyint(1)"),
    };

    internal readonly static List<Column> guild_users = new()
    {
        new Column("userid", "bigint", primary: true),
        new Column("saved_nickname", "text", "utf8mb4_0900_ai_ci", "", true),
        new Column("roles", "text", "utf8mb4_0900_ai_ci"),
        new Column("first_join", "bigint"),
        new Column("last_leave", "bigint"),
        new Column("experience_last_message", "bigint"),
        new Column("experience", "bigint"),
        new Column("experience_level", "bigint"),
    };

    internal readonly static Dictionary<string, List<Column>> Tables = new()
    {
        { "scam_urls", scam_urls },
        { "globalbans", globalbans },
        { "user_submission_bans", user_submission_bans },
        { "guild_submission_bans", guild_submission_bans },
        { "active_url_submissions", active_url_submissions },
        { "users", users },
        { "guilds", guilds },
        { "writetester", writetester },
    };

    public class Column
    {
        public Column(string name, string type, string Collation = "", string Default = "", bool nullable = false, bool primary = false)
        {
            this.Name = name;
            this.Type = type;
            this.Collation = Collation;
            this.Default = Default;
            this.Nullable = nullable;
            this.Primary = primary;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Collation { get; set; }
        public string Default { get; set; }
        public bool Nullable { get; set; }
        public bool Primary { get; set; }
    }
}
