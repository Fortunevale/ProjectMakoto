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

public sealed class UrlSubmissionSettings : RequiresParent<User>
{
    public UrlSubmissionSettings(Bot bot, User parent) : base(bot, parent)
    {
    }

    [ColumnName("submission_accepted_tos"), ColumnType(ColumnTypes.TinyInt), Default("0")]
    public int AcceptedTOS
    {
        get => this.Bot.DatabaseClient.GetValue<int>("users", "userid", this.Parent.Id, "submission_accepted_tos", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "submission_accepted_tos", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("submission_last_datetime"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastTime
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("users", "userid", this.Parent.Id, "submission_last_datetime", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "submission_last_datetime", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}