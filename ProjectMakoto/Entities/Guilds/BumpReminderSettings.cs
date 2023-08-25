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

public sealed class BumpReminderSettings : RequiresParent<Guild>
{
    public BumpReminderSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    public void Reset()
    {
        this.ChannelId = 0;
        this.RoleId = 0;
        this.MessageId = 0;
        this.PersistentMessageId = 0;
        this.LastUserId = 0;
        this.LastBump = DateTime.MinValue;
        this.LastReminder = DateTime.MinValue;
        this.BumpsMissed = 0;
    }

    [ColumnName("bump_channel"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong ChannelId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "bump_channel", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_channel", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_role"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong RoleId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "bump_role", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_role", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }


    [ColumnName("bump_message"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong MessageId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "bump_message", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_message", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_persistent_msg"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong PersistentMessageId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "bump_persistent_msg", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_persistent_msg", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_last_user"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong LastUserId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("guilds", "serverid", this.Parent.Id, "bump_last_user", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_last_user", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_last_time"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastBump
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("guilds", "serverid", this.Parent.Id, "bump_last_time", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_last_time", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_last_reminder"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastReminder
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("guilds", "serverid", this.Parent.Id, "bump_last_reminder", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_last_reminder", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("bump_missed"), ColumnType(ColumnTypes.Int), Default("0")]
    public int BumpsMissed
    {
        get => this.Bot.DatabaseClient.GetValue<int>("guilds", "serverid", this.Parent.Id, "bump_missed", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "bump_missed", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}