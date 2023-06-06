// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class InVoiceTextPrivacySettings
{
    public InVoiceTextPrivacySettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _ClearTextEnabled { get; set; } = false;
    public bool ClearTextEnabled 
    { 
        get => _ClearTextEnabled; 
        set 
        { 
            _ClearTextEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "vc_privacy_clear", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    private bool _SetPermissionsEnabled { get; set; } = false;
    public bool SetPermissionsEnabled 
    { 
        get => _SetPermissionsEnabled; 
        set 
        { 
            _SetPermissionsEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "vc_privacy_perms", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}