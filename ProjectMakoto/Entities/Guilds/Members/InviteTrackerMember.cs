// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Entities.Members;

public sealed class InviteTrackerMember : RequiresParent<Member>
{
    public InviteTrackerMember(Bot bot, Member parent) : base(bot, parent)
    {
    }

    [ColumnName("invite_user"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong UserId
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_user", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_user", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("invite_code"), ColumnType(ColumnTypes.Text), Default("")]
    public string Code
    {
        get => this.Bot.DatabaseClient.GetValue<string>(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_code", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_code", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }
}
