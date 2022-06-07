namespace Project_Ichigo.Objects;

internal class ActionLogSettings
{
    private ulong _Channel { get; set; } = 0;
    public ulong Channel { get => _Channel; set { _Channel = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _AttemptGettingMoreDetails { get; set; } = false;
    public bool AttemptGettingMoreDetails { get => _AttemptGettingMoreDetails; set { _AttemptGettingMoreDetails = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _MembersModified { get; set; } = false;
    public bool MembersModified { get => _MembersModified; set { _MembersModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _MemberModified { get; set; } = false;
    public bool MemberModified { get => _MemberModified; set { _MemberModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _MemberProfileModified { get; set; } = false;
    public bool MemberProfileModified { get => _MemberProfileModified; set { _MemberProfileModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _MessageDeleted { get; set; } = false;
    public bool MessageDeleted { get => _MessageDeleted; set { _MessageDeleted = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _MessageModified { get; set; } = false;
    public bool MessageModified { get => _MessageModified; set { _MessageModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _RolesModified { get; set; } = false;
    public bool RolesModified { get => _RolesModified; set { _RolesModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _BanlistModified { get; set; } = false;
    public bool BanlistModified { get => _BanlistModified; set { _BanlistModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _GuildModified { get; set; } = false;
    public bool GuildModified { get => _GuildModified; set { _GuildModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _ChannelsModified { get; set; } = false;
    public bool ChannelsModified { get => _ChannelsModified; set { _ChannelsModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _VoiceStateUpdated { get; set; } = false;
    public bool VoiceStateUpdated { get => _VoiceStateUpdated; set { _VoiceStateUpdated = value; _ = Bot.DatabaseClient.SyncDatabase(); } }

    private bool _InvitesModified { get; set; } = false;
    public bool InvitesModified { get => _InvitesModified; set { _InvitesModified = value; _ = Bot.DatabaseClient.SyncDatabase(); } }
}
