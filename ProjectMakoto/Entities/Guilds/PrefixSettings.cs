// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;
public sealed class PrefixSettings : RequiresParent<Guild>
{
    public PrefixSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    private string _Prefix { get; set; }
    public string Prefix
    {
        get => this._Prefix.IsNullOrWhiteSpace() ? this.Bot.Prefix : this._Prefix; set
        {
            this._Prefix = value.IsNullOrWhiteSpace() ? this.Bot.Prefix : value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "prefix", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _PrefixDisabled { get; set; } = false;
    public bool PrefixDisabled
    {
        get => this._PrefixDisabled; set
        {
            this._PrefixDisabled = value;
            _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "prefix_disabled", value, this.Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
