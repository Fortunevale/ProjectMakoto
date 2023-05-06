// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class NameNormalizerSettings
{
    public NameNormalizerSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _NameNormalizerEnabled { get; set; } = false;
    public bool NameNormalizerEnabled 
    { 
        get => _NameNormalizerEnabled; 
        set 
        { 
            _NameNormalizerEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "normalizenames", value, Bot.DatabaseClient.mainDatabaseConnection);
        } 
    }

    public bool NameNormalizerRunning = false;
}
