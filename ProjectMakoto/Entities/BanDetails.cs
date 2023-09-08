// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

[TableName("-")]
public sealed class BanDetails : RequiresBotReference
{
    private string _tableName;

    public BanDetails(Bot bot, string tableName, ulong Id) : base(bot)
    {
        this.Id = Id;

        this._tableName = tableName;

        _ = this.Bot.DatabaseClient.CreateRow(this._tableName, typeof(BanDetails), Id, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("id"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }

    [ColumnName("reason"), ColumnType(ColumnTypes.LongText), WithCollation, Default("-")]
    public string Reason
    {
        get => this.Bot.DatabaseClient.GetValue<string>(this._tableName, "id", this.Id, "reason", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this._tableName, "id", this.Id, "reason", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("moderator"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong Moderator
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>(this._tableName, "id", this.Id, "moderator", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this._tableName, "id", this.Id, "moderator", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("timestamp"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime Timestamp
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this._tableName, "id", this.Id, "timestamp", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this._tableName, "id", this.Id, "timestamp", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
