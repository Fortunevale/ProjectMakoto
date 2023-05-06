// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class BumpReminderSettings
{
    public BumpReminderSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _Enabled { get; set; } = false;
    public bool Enabled 
    { 
        get => _Enabled; 
        set 
        { 
            _Enabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_enabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _RoleId { get; set; } = 0;
    public ulong RoleId 
    { 
        get => _RoleId; 
        set 
        { 
            _RoleId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_role", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId 
    { 
        get => _ChannelId; 
        set 
        { 
            _ChannelId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_channel", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _MessageId { get; set; } = 0;
    public ulong MessageId 
    { 
        get => _MessageId; 
        set 
        { 
            _MessageId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_message", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _PersistentMessageId { get; set; } = 0;
    public ulong PersistentMessageId 
    { 
        get => _PersistentMessageId; 
        set 
        { 
            _PersistentMessageId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_persistent_msg", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private ulong _LastUserId { get; set; } = 0;
    public ulong LastUserId 
    { 
        get => _LastUserId; 
        set 
        { 
            _LastUserId = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_last_user", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private DateTime _LastBump { get; set; } = DateTime.MinValue;
    public DateTime LastBump 
    { 
        get => _LastBump; 
        set 
        { 
            _LastBump = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_last_time", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private DateTime _LastReminder { get; set; } = DateTime.MinValue;
    public DateTime LastReminder 
    { 
        get => _LastReminder; 
        set 
        { 
            _LastReminder = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_last_reminder", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
    
    private int _BumpsMissed { get; set; } = 0;
    public int BumpsMissed 
    { 
        get => _BumpsMissed; 
        set 
        { 
            _BumpsMissed = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "bump_missed", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}