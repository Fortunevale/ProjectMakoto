// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class ActionLogSettings : RequiresParent<Guild>
{
    public ActionLogSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    public ObservableList<ulong> ProcessedAuditLogs { get => this._ProcessedAuditLogs; set { this._ProcessedAuditLogs = value; this._ProcessedAuditLogs.ItemsChanged += this.AuditLogCollectionUpdated; } }
    private ObservableList<ulong> _ProcessedAuditLogs { get; set; } = new();

    private ulong _Channel { get; set; } = 0;
    public ulong Channel
    {
        get => this._Channel; set
        {
            this._Channel = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_channel", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _AttemptGettingMoreDetails { get; set; } = false;
    public bool AttemptGettingMoreDetails
    {
        get => this._AttemptGettingMoreDetails;
        set
        {
            this._AttemptGettingMoreDetails = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_attempt_further_detail", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _MembersModified { get; set; } = false;
    public bool MembersModified
    {
        get => this._MembersModified;
        set
        {
            this._MembersModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_members_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _MemberModified { get; set; } = false;
    public bool MemberModified
    {
        get => this._MemberModified;
        set
        {
            this._MemberModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_member_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _MemberProfileModified { get; set; } = false;
    public bool MemberProfileModified
    {
        get => this._MemberProfileModified;
        set
        {
            this._MemberProfileModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_memberprofile_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _MessageDeleted { get; set; } = false;
    public bool MessageDeleted
    {
        get => this._MessageDeleted;
        set
        {
            this._MessageDeleted = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_message_deleted", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _MessageModified { get; set; } = false;
    public bool MessageModified
    {
        get => this._MessageModified;
        set
        {
            this._MessageModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_message_updated", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _RolesModified { get; set; } = false;
    public bool RolesModified
    {
        get => this._RolesModified;
        set
        {
            this._RolesModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_roles_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _BanlistModified { get; set; } = false;
    public bool BanlistModified
    {
        get => this._BanlistModified;
        set
        {
            this._BanlistModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_banlist_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _GuildModified { get; set; } = false;
    public bool GuildModified
    {
        get => this._GuildModified;
        set
        {
            this._GuildModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_guild_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _ChannelsModified { get; set; } = false;
    public bool ChannelsModified
    {
        get => this._ChannelsModified;
        set
        {
            this._ChannelsModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_channels_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _VoiceStateUpdated { get; set; } = false;
    public bool VoiceStateUpdated
    {
        get => this._VoiceStateUpdated;
        set
        {
            this._VoiceStateUpdated = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_voice_state", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _InvitesModified { get; set; } = false;
    public bool InvitesModified
    {
        get => this._InvitesModified;
        set
        {
            this._InvitesModified = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "actionlog_log_invites_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private void AuditLogCollectionUpdated(object? sender, ObservableListUpdate<ulong> e)
    {
        while (this.ProcessedAuditLogs.Count > 50)
        {
            this.ProcessedAuditLogs.RemoveAt(0);
        }
    }
}
