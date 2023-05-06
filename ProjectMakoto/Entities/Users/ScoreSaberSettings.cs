// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class ScoreSaberSettings
{
    public ScoreSaberSettings(User user)
    {
        Parent = user;
    }
    private User Parent { get; set; }



    private ulong _Id { get; set; } = 0;
    public ulong Id 
    { 
        get => _Id; 
        set 
        { 
            _Id = value;
            _ = Bot.DatabaseClient.UpdateValue("users", "userid", Parent.UserId, "scoresaber_id", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
