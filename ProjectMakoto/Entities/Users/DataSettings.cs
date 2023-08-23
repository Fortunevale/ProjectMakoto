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
public sealed class DataSettings : RequiresParent<User>
{
    public DataSettings(Bot bot, User parent) : base(bot, parent)
    {
    }

    [ColumnName("last_data_request"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastDataRequest
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("users", "userid", this.Parent.Id, "last_data_request", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "last_data_request", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("deletion_requested"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public bool DeletionRequested
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("users", "userid", this.Parent.Id, "deletion_requested", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "deletion_requested", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("data_deletion_date"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime DeletionRequestDate
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("users", "userid", this.Parent.Id, "data_deletion_date", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "data_deletion_date", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
