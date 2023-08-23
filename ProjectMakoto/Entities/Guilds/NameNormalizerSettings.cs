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

public sealed class NameNormalizerSettings : RequiresParent<Guild>
{
    public NameNormalizerSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("normalizenames"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool NameNormalizerEnabled
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "normalizenames", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "normalizenames", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    public bool NameNormalizerRunning = false;
}
