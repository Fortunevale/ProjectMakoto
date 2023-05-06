// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class Member
{
    public Member(Guild guild, ulong member)
    {
        Guild = guild;
        MemberId = member;

        InviteTracker = new(this);
        Experience = new(this);
    }

    [JsonIgnore]
    internal Guild Guild { get; set; }
    [JsonIgnore]
    internal ulong MemberId { get; set; }


    private string _SavedNickname { get; set; } = "";
    public string SavedNickname 
    { 
        get => _SavedNickname; 
        set 
        { 
            _SavedNickname = value;
            _ = Bot.DatabaseClient.UpdateValue(Guild.ServerId.ToString(), "userid", MemberId, "saved_nickname", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }



    private DateTime _FirstJoinDate { get; set; } = DateTime.UnixEpoch;
    public DateTime FirstJoinDate 
    { 
        get => _FirstJoinDate; 
        set 
        { 
            _FirstJoinDate = value;
            _ = Bot.DatabaseClient.UpdateValue(Guild.ServerId.ToString(), "userid", MemberId, "first_join", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }



    private DateTime _LastLeaveDate { get; set; } = DateTime.UnixEpoch;
    public DateTime LastLeaveDate 
    { 
        get => _LastLeaveDate; 
        set 
        { 
            _LastLeaveDate = value;
            _ = Bot.DatabaseClient.UpdateValue(Guild.ServerId.ToString(), "userid", MemberId, "last_leave", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }



    public InviteTrackerMember InviteTracker { get; set; }

    public ExperienceMember Experience { get; set; }

    public List<MemberRole> MemberRoles { get; set; } = new();
}
