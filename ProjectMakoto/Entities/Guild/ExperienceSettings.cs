// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class ExperienceSettings
{
    public ExperienceSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _UseExperience { get; set; } = false;
    public bool UseExperience 
    { 
        get => _UseExperience; 
        set 
        { 
            _UseExperience = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "experience_use", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }



    private bool _BoostXpForBumpReminder { get; set; } = false;
    public bool BoostXpForBumpReminder 
    { 
        get => _BoostXpForBumpReminder; 
        set 
        { 
            _BoostXpForBumpReminder = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "experience_boost_bumpreminder", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }
}
