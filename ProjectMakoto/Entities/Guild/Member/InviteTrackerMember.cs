// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class InviteTrackerMember
{
    public InviteTrackerMember(Member member)
    {
        Parent = member;
    }

    private Member Parent { get; set; }



    private ulong _UserId { get; set; } = 0;
    public ulong UserId 
    { 
        get => _UserId; 
        set 
        { 
            _UserId = value;
            _ = Bot.DatabaseClient.UpdateValue(Parent.Guild.ServerId.ToString(), "userid", Parent.MemberId, "invite_user", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }

    private string _Code { get; set; } = "";
    public string Code 
    { 
        get => _Code; 
        set 
        { 
            _Code = value;
            _ = Bot.DatabaseClient.UpdateValue(Parent.Guild.ServerId.ToString(), "userid", Parent.MemberId, "invite_code", value, Bot.DatabaseClient.guildDatabaseConnection);
        } 
    }
}
