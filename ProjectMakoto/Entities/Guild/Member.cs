// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Member : BaseSelfFillingList
{
    public Member(Bot bot, Guild guild, ulong key) : base(bot, key)
    {
        this.InviteTracker = new(bot, this);
        this.Experience = new(bot, this);
        this.Parent = guild;
    }

    [JsonIgnore]
    internal Guild Parent { get; set; }


    private string _SavedNickname { get; set; } = "";
    public string SavedNickname
    {
        get => this._SavedNickname;
        set
        {
            this._SavedNickname = value;
            _ = Bot.DatabaseClient.UpdateValue(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", value, Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    private DateTime _FirstJoinDate { get; set; } = DateTime.UnixEpoch;
    public DateTime FirstJoinDate
    {
        get => this._FirstJoinDate;
        set
        {
            this._FirstJoinDate = value;
            _ = Bot.DatabaseClient.UpdateValue(this.Parent.Id.ToString(), "userid", this.Id, "first_join", value, Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    private DateTime _LastLeaveDate { get; set; } = DateTime.UnixEpoch;
    public DateTime LastLeaveDate
    {
        get => this._LastLeaveDate;
        set
        {
            this._LastLeaveDate = value;
            _ = Bot.DatabaseClient.UpdateValue(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", value, Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    public InviteTrackerMember InviteTracker { get; set; }

    public ExperienceMember Experience { get; set; }

    public List<MemberRole> MemberRoles { get; set; } = new();
}
