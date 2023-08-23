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

public sealed class InVoiceTextPrivacySettings : RequiresParent<Guild>
{
    public InVoiceTextPrivacySettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("vc_privacy_clear"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool ClearTextEnabled
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "vc_privacy_clear", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "vc_privacy_clear", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("vc_privacy_perms"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool SetPermissionsEnabled
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "vc_privacy_perms", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "vc_privacy_perms", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}