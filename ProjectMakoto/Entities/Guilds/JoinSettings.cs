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

public sealed class JoinSettings : RequiresParent<Guild>
{
    public JoinSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("auto_assign_role_id"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong AutoAssignRoleId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "auto_assign_role_id", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "auto_assign_role_id", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("joinlog_channel_id"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong JoinlogChannelId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "joinlog_channel_id", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "joinlog_channel_id", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("autoban_global_ban"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool AutoBanGlobalBans
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "autoban_global_ban", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "autoban_global_ban", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("reapplyroles"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool ReApplyRoles
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "reapplyroles", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "reapplyroles", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("reapplynickname"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool ReApplyNickname
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "reapplynickname", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "reapplynickname", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}