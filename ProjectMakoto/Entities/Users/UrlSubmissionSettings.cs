// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class UrlSubmissionSettings
{
    public UrlSubmissionSettings(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private int _AcceptedTOS { get; set; } = 0;
    public int AcceptedTOS 
    { 
        get => _AcceptedTOS; 
        set 
        { 
            _AcceptedTOS = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "submission_accepted_tos", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    private DateTime _LastTime { get; set; } = DateTime.MinValue;
    public DateTime LastTime 
    { 
        get => _LastTime; 
        set 
        { 
            _LastTime = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "submission_last_datetime", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }


    public List<string> AcceptedSubmissions { get; set; } = new();
}