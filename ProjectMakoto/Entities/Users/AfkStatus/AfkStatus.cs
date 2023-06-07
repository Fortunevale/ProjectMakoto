// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class AfkStatus
{
    public AfkStatus(User user)
    {
        this.Parent = user;
    }
    private User Parent { get; set; }



    private string _Reason { get; set; } = "";
    public string Reason
    {
        get => this._Reason;
        set
        {
            this._Reason = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.UserId, "afk_reason", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }



    private DateTime _TimeStamp { get; set; } = DateTime.UnixEpoch;
    public DateTime TimeStamp
    {
        get => this._TimeStamp;
        set
        {
            this._TimeStamp = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.UserId, "afk_since", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private long _MessagesAmount { get; set; } = 0;
    public long MessagesAmount
    {
        get => this._MessagesAmount;
        set
        {
            this._MessagesAmount = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.UserId, "afk_pingamount", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }



    private List<MessageDetails> _Messages { get; set; } = new();
    public List<MessageDetails> Messages
    {
        get
        {
            this._Messages ??= new();

            return this._Messages;
        }

        set
        {
            this._Messages = value;
        }
    }

    [JsonIgnore]
    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
