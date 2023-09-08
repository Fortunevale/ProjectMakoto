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
public sealed class PrefixSettings : RequiresParent<Guild>
{
    public PrefixSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("prefix"), ColumnType(ColumnTypes.Text), WithCollation, Default(";;")]
    public string Prefix
    {
        get => this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "prefix", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "prefix", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("prefix_disabled"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool PrefixDisabled
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "prefix_disabled", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "prefix_disabled", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
