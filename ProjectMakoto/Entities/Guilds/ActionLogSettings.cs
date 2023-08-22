// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class ActionLogSettings : RequiresParent<Guild>
{
    public ActionLogSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("auditlogcache"), ColumnType(ColumnTypes.LongText)]
    public ulong[] ProcessedAuditLogs
    {
        get => JsonConvert.DeserializeObject<ulong[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "auditlogcache", this.Bot.DatabaseClient.mainDatabaseConnection));
        set
        {
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "auditlogcache", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
            this.AuditLogCollectionUpdated();
        }
    }

    [ColumnName("actionlog_channel"), ColumnType(ColumnTypes.BigInt)]
    public ulong Channel
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "actionlog_channel", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_channel", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_attempt_further_detail"), ColumnType(ColumnTypes.TinyInt)]
    public bool AttemptGettingMoreDetails
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_attempt_further_detail", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_attempt_further_detail", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_members_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool MembersModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_members_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_members_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_member_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool MemberModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_member_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_member_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_memberprofile_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool MemberProfileModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_memberprofile_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_memberprofile_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_message_deleted"), ColumnType(ColumnTypes.TinyInt)]
    public bool MessageDeleted
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_message_deleted", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_message_deleted", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_message_updated"), ColumnType(ColumnTypes.TinyInt)]
    public bool MessageModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_message_updated", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_message_updated", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_roles_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool RolesModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_roles_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_roles_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_banlist_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool BanlistModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_banlist_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_banlist_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_guild_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool GuildModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_guild_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_guild_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_channels_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool ChannelsModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_channels_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_channels_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_voice_state"), ColumnType(ColumnTypes.TinyInt)]
    public bool VoiceStateUpdated
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_voice_state", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_voice_state", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("actionlog_log_invites_modified"), ColumnType(ColumnTypes.TinyInt)]
    public bool InvitesModified
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "actionlog_log_invites_modified", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "actionlog_log_invites_modified", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    private void AuditLogCollectionUpdated()
    {
        while (this.ProcessedAuditLogs.Length > 50)
        {
            this.ProcessedAuditLogs = this.ProcessedAuditLogs.Skip(1).ToArray();
        }
    }
}
