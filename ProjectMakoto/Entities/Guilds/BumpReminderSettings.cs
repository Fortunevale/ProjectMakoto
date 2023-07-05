// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class BumpReminderSettings : RequiresParent<Guild>
{
    public BumpReminderSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    private bool _Enabled { get; set; } = false;
    public bool Enabled
    {
        get => this._Enabled;
        set
        {
            this._Enabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_enabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _RoleId { get; set; } = 0;
    public ulong RoleId
    {
        get => this._RoleId;
        set
        {
            this._RoleId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_role", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId
    {
        get => this._ChannelId;
        set
        {
            this._ChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_channel", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _MessageId { get; set; } = 0;
    public ulong MessageId
    {
        get => this._MessageId;
        set
        {
            this._MessageId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_message", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _PersistentMessageId { get; set; } = 0;
    public ulong PersistentMessageId
    {
        get => this._PersistentMessageId;
        set
        {
            this._PersistentMessageId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_persistent_msg", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private ulong _LastUserId { get; set; } = 0;
    public ulong LastUserId
    {
        get => this._LastUserId;
        set
        {
            this._LastUserId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_last_user", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private DateTime _LastBump { get; set; } = DateTime.MinValue;
    public DateTime LastBump
    {
        get => this._LastBump;
        set
        {
            this._LastBump = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_last_time", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private DateTime _LastReminder { get; set; } = DateTime.MinValue;
    public DateTime LastReminder
    {
        get => this._LastReminder;
        set
        {
            this._LastReminder = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_last_reminder", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private int _BumpsMissed { get; set; } = 0;
    public int BumpsMissed
    {
        get => this._BumpsMissed;
        set
        {
            this._BumpsMissed = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "bump_missed", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}