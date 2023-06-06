// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
public sealed class PrefixSettings
{
    public PrefixSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }

    private string _Prefix { get; set; }
    public string Prefix
    {
        get => _Prefix.IsNullOrWhiteSpace() ? Parent._bot.Prefix : _Prefix; set
        {
            _Prefix = value.IsNullOrWhiteSpace() ? Parent._bot.Prefix : value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "prefix", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _PrefixDisabled { get; set; } = false;
    public bool PrefixDisabled
    {
        get => _PrefixDisabled; set
        {
            _PrefixDisabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "prefix_disabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
