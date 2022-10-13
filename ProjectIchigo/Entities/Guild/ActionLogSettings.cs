namespace ProjectIchigo.Entities;

public class ActionLogSettings
{
    public ActionLogSettings(Guild guild)
    {
        Parent = guild;

        _ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated;
    }

    private Guild Parent { get; set; }

    public ObservableList<ulong> ProcessedAuditLogs { get => _ProcessedAuditLogs; set { _ProcessedAuditLogs = value; _ProcessedAuditLogs.ItemsChanged += AuditLogCollectionUpdated; } }
    private ObservableList<ulong> _ProcessedAuditLogs { get; set; } = new();

    private ulong _Channel { get; set; } = 0;
    public ulong Channel 
    { 
        get => _Channel; set 
        { 
            _Channel = value; 
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_channel", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _AttemptGettingMoreDetails { get; set; } = false;
    public bool AttemptGettingMoreDetails 
    { 
        get => _AttemptGettingMoreDetails;
        set 
        { 
            _AttemptGettingMoreDetails = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_attempt_further_detail", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _MembersModified { get; set; } = false;
    public bool MembersModified 
    { 
        get => _MembersModified; 
        set 
        { 
            _MembersModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_members_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _MemberModified { get; set; } = false;
    public bool MemberModified 
    { 
        get => _MemberModified; 
        set 
        { 
            _MemberModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_member_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _MemberProfileModified { get; set; } = false;
    public bool MemberProfileModified 
    { 
        get => _MemberProfileModified; 
        set 
        { 
            _MemberProfileModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_memberprofile_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _MessageDeleted { get; set; } = false;
    public bool MessageDeleted 
    { 
        get => _MessageDeleted; 
        set 
        { 
            _MessageDeleted = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_message_deleted", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _MessageModified { get; set; } = false;
    public bool MessageModified 
    { 
        get => _MessageModified; 
        set 
        { 
            _MessageModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_message_updated", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _RolesModified { get; set; } = false;
    public bool RolesModified 
    { 
        get => _RolesModified; 
        set 
        { 
            _RolesModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_roles_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _BanlistModified { get; set; } = false;
    public bool BanlistModified 
    { 
        get => _BanlistModified; 
        set 
        { 
            _BanlistModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_banlist_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _GuildModified { get; set; } = false;
    public bool GuildModified 
    { 
        get => _GuildModified; 
        set 
        { 
            _GuildModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_guild_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _ChannelsModified { get; set; } = false;
    public bool ChannelsModified 
    { 
        get => _ChannelsModified; 
        set 
        { 
            _ChannelsModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_channels_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _VoiceStateUpdated { get; set; } = false;
    public bool VoiceStateUpdated 
    { 
        get => _VoiceStateUpdated; 
        set 
        { 
            _VoiceStateUpdated = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_voice_state", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _InvitesModified { get; set; } = false;
    public bool InvitesModified 
    { 
        get => _InvitesModified; 
        set 
        { 
            _InvitesModified = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "actionlog_log_invites_modified", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private void AuditLogCollectionUpdated(object? sender, ObservableListUpdate<ulong> e)
    {
        while (ProcessedAuditLogs.Count > 50)
        {
            ProcessedAuditLogs.RemoveAt(0);
        }
    }
}
