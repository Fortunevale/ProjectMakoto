// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Users;

public sealed class ScoreSaberSettings(Bot bot, User parent) : RequiresParent<User>(bot, parent)
{
    [ColumnName("scoresaber_id"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong Id
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("users", "userid", this.Parent.Id, "scoresaber_id", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "scoresaber_id", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
