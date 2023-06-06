// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class NameNormalizerSettings
{
    public NameNormalizerSettings(Guild guild)
    {
        this.Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _NameNormalizerEnabled { get; set; } = false;
    public bool NameNormalizerEnabled
    {
        get => this._NameNormalizerEnabled;
        set
        {
            this._NameNormalizerEnabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "normalizenames", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    public bool NameNormalizerRunning = false;
}
