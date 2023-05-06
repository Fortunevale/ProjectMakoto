// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class AfkStatus
{
    public AfkStatus(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private string _Reason { get; set; } = "";
    public string Reason 
    { 
        get => _Reason; 
        set 
        { 
            _Reason = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_reason", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private DateTime _TimeStamp { get; set; } = DateTime.UnixEpoch;
    public DateTime TimeStamp 
    { 
        get => _TimeStamp; 
        set 
        { 
            _TimeStamp = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_since", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private long _MessagesAmount { get; set; } = 0;
    public long MessagesAmount 
    { 
        get => _MessagesAmount; 
        set 
        { 
            _MessagesAmount = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "afk_pingamount", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private List<MessageDetails> _Messages { get; set; } = new();
    public List<MessageDetails> Messages
    {
        get
        {
            _Messages ??= new();

            return _Messages;
        }

        set
        {
            _Messages = value;
        }
    }

    [JsonIgnore]
    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
