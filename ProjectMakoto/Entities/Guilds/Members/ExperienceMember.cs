// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Entities.Members;

public sealed class ExperienceMember(Bot bot, Member parent) : RequiresParent<Member>(bot, parent)
{
    [ColumnName("experience_last_message"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime Last_Message
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_last_message", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_last_message", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("experience"), ColumnType(ColumnTypes.BigInt), Default("1")]
    public long Points
    {
        get => this.Bot.DatabaseClient.GetValue<long>(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("experience_level"), ColumnType(ColumnTypes.BigInt), Default("1")]
    public long Level
    {
        get => this.Bot.DatabaseClient.GetValue<long>(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_level", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_level", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }
}
