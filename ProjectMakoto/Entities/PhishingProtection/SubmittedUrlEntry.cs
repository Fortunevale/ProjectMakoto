// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

[TableName("active_url_submissions")] 
internal sealed class SubmittedUrlEntry : RequiresBotReference
{
    public SubmittedUrlEntry(Bot bot, ulong Id) : base(bot)
    {
        this.Id = Id;

        _ = this.Bot.DatabaseClient.CreateRow("active_url_submissions", typeof(SubmittedUrlEntry), Id, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("messageid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }

    [ColumnName("url"), ColumnType(ColumnTypes.LongText), Default("")]
    public string Url
    {
        get => this.Bot.DatabaseClient.GetValue<string>("active_url_submissions", "messageid", this.Id, "url", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("active_url_submissions", "messageid", this.Id, "url", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("submitter"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong Submitter
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("active_url_submissions", "messageid", this.Id, "submitter", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("active_url_submissions", "messageid", this.Id, "submitter", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("guild"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong GuildOrigin
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("active_url_submissions", "messageid", this.Id, "guild", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("active_url_submissions", "messageid", this.Id, "guild", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
