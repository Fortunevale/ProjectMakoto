// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Entities.Guilds;

public sealed class Member : RequiresParent<Guild>
{
    public Member(Bot bot, Guild guild, ulong key) : base(bot, guild)
    {
        this.InviteTracker = new(bot, this);
        this.Experience = new(bot, this);
        this.Id = key;
    }

    internal ulong Id { get; set; }

    private string _SavedNickname { get; set; } = "";
    public string SavedNickname
    {
        get => this._SavedNickname;
        set
        {
            this._SavedNickname = value;
            _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "saved_nickname", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    private DateTime _FirstJoinDate { get; set; } = DateTime.UnixEpoch;
    public DateTime FirstJoinDate
    {
        get => this._FirstJoinDate;
        set
        {
            this._FirstJoinDate = value;
            _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "first_join", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    private DateTime _LastLeaveDate { get; set; } = DateTime.UnixEpoch;
    public DateTime LastLeaveDate
    {
        get => this._LastLeaveDate;
        set
        {
            this._LastLeaveDate = value;
            _ = this.Bot.DatabaseClient.SetValue(this.Parent.Id.ToString(), "userid", this.Id, "last_leave", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }



    public InviteTrackerMember InviteTracker { get; set; }

    public ExperienceMember Experience { get; set; }

    public List<MemberRole> MemberRoles { get; set; } = new();
}
