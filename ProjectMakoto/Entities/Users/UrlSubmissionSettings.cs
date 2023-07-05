// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class UrlSubmissionSettings : RequiresParent<User>
{
    public UrlSubmissionSettings(Bot bot, User parent) : base(bot, parent)
    {
    }

    private int _AcceptedTOS { get; set; } = 0;
    public int AcceptedTOS
    {
        get => this._AcceptedTOS;
        set
        {
            this._AcceptedTOS = value;
            _ = this.Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "submission_accepted_tos", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    private DateTime _LastTime { get; set; } = DateTime.MinValue;
    public DateTime LastTime
    {
        get => this._LastTime;
        set
        {
            this._LastTime = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", this.Parent.Id, "submission_last_datetime", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }


    public List<string> AcceptedSubmissions { get; set; } = new();
}