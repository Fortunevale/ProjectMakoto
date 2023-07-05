// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class InviteTrackerSettings : RequiresParent<Guild>
{
    public InviteTrackerSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    private bool _Enabled { get; set; } = false;
    public bool Enabled
    {
        get => this._Enabled;
        set
        {
            this._Enabled = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "invitetracker_enabled", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private List<InviteTrackerCacheItem> _Cache { get; set; } = new();
    public List<InviteTrackerCacheItem> Cache
    {
        get => this._Cache;
        set
        {
            this._Cache = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.Id, "invitetracker_cache", JsonConvert.SerializeObject(value), Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
