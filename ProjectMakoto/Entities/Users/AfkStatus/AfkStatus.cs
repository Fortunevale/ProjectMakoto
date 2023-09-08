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

public sealed class AfkStatus : RequiresParent<User>
{
    public AfkStatus(Bot bot, User parent) : base(bot, parent)
    {
    }

    [ColumnName("afk_reason"), ColumnType(ColumnTypes.Text), WithCollation, Nullable]
    public string Reason
    {
        get => this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "afk_reason", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "afk_reason", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("afk_since"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime TimeStamp
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>("users", "userid", this.Parent.Id, "afk_since", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "afk_since", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
    [ColumnName("afk_pingamount"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public long MessagesAmount
    {
        get => this.Bot.DatabaseClient.GetValue<long>("users", "userid", this.Parent.Id, "afk_pingamount", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "afk_pingamount", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("afk_pings"), ColumnType(ColumnTypes.Text), WithCollation, Default("[]")]
    public MessageDetails[] Messages
    {
        get => JsonConvert.DeserializeObject<MessageDetails[]>(this.Bot.DatabaseClient.GetValue<string>("users", "userid", this.Parent.Id, "afk_pings", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("users", "userid", this.Parent.Id, "afk_pings", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [JsonIgnore]
    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
