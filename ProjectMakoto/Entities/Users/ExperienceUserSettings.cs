// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class ExperienceUserSettings(Bot bot, User parent) : RequiresParent<User>(bot, parent)
{
    [ColumnName("experience_directmessageoptout"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool DirectMessageOptOut
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("users", "userid", this.Parent.Id, "experience_directmessageoptout", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "experience_directmessageoptout", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
