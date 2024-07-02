// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;
public class TranslationReportSettings(Bot bot, User parent) : RequiresParent<User>(bot, parent)
{
    [ColumnName("translationreport_accepted_tos"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public int AcceptedTOS
    {
        get => this.Bot.DatabaseClient.GetValue<int>("users", "userid", this.Parent.Id, "translationreport_accepted_tos", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "translationreport_accepted_tos", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("translationreport_ratelimit_first"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime FirstRequestTime
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("users", "userid", this.Parent.Id, "translationreport_ratelimit_first", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "translationreport_ratelimit_first", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("translationreport_ratelimit_count"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public int RequestCount
    {
        get => this.Bot.DatabaseClient.GetValue<int>("users", "userid", this.Parent.Id, "translationreport_ratelimit_count", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "translationreport_ratelimit_count", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
