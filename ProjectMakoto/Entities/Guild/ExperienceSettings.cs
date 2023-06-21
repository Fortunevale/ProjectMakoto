// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class ExperienceSettings : RequiresParent<Guild>
{
    public ExperienceSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    private bool _UseExperience { get; set; } = false;
    public bool UseExperience
    {
        get => this._UseExperience;
        set
        {
            this._UseExperience = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "experience_use", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }



    private bool _BoostXpForBumpReminder { get; set; } = false;
    public bool BoostXpForBumpReminder
    {
        get => this._BoostXpForBumpReminder;
        set
        {
            this._BoostXpForBumpReminder = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "experience_boost_bumpreminder", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
