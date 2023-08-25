// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Entities.Guilds;

[TableName("-")]
public sealed class Member : RequiresParent<Guild>
{
    public Member(Bot bot, Guild guild, ulong key) : base(bot, guild)
    {
        this.InviteTracker = new(bot, this);
        this.Experience = new(bot, this);
        this.Id = key;

        _ = this.Bot.DatabaseClient.CreateRow(this.Parent.Id.ToString(), typeof(Member), key, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("userid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; set; }

    [ColumnName("saved_nickname"), ColumnType(ColumnTypes.Text), Collation("utf8mb4_0900_ai_ci"), Nullable]
    public string? SavedNickname
    {
        get => this.Bot.DatabaseClient.GetValue<string>(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("first_join"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime FirstJoinDate
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this.Parent.Id.ToString(), "userid", this.Id, "first_join", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "first_join", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("last_leave"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime LastLeaveDate
    {
        get => this.Bot.DatabaseClient.GetValue<DateTime>(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", this.Bot.DatabaseClient.guildDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", value, this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ColumnName("roles"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Default("[]")]
    public MemberRole[] MemberRoles
    {
        get => JsonConvert.DeserializeObject<MemberRole[]>(this.Bot.DatabaseClient.GetValue<string>(this.Parent.Id.ToString(), "userid", this.Id, "roles", this.Bot.DatabaseClient.guildDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "roles", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.guildDatabaseConnection);
    }

    [ContainsValues]
    public InviteTrackerMember InviteTracker { get; init; }

    [ContainsValues]
    public ExperienceMember Experience { get; init; }
}