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

public sealed class ExperienceSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("experience_use"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool UseExperience
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "experience_use", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "experience_use", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("experience_boost_bumpreminder"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool BoostXpForBumpReminder
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "experience_boost_bumpreminder", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "experience_boost_bumpreminder", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
