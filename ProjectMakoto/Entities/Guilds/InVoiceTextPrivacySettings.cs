// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class InVoiceTextPrivacySettings : RequiresParent<Guild>
{
    public InVoiceTextPrivacySettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    private bool _ClearTextEnabled { get; set; } = false;
    public bool ClearTextEnabled
    {
        get => this._ClearTextEnabled;
        set
        {
            this._ClearTextEnabled = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "vc_privacy_clear", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _SetPermissionsEnabled { get; set; } = false;
    public bool SetPermissionsEnabled
    {
        get => this._SetPermissionsEnabled;
        set
        {
            this._SetPermissionsEnabled = value;
            _ = this.Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "vc_privacy_perms", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}