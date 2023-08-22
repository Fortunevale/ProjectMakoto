// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class InviteTrackerCacheItem
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public Guild Parent { get; set; }

    private ulong _CreatorId { get; set; }
    public ulong CreatorId
    {
        get => this._CreatorId;
        set
        {
            this._CreatorId = value;
            this.Update();
        }
    }


    private string _Code { get; set; }
    public string Code
    {
        get => this._Code;
        set
        {
            this._Code = value;
            this.Update();
        }
    }

    private long _Uses { get; set; }
    public long Uses
    {
        get => this._Uses;
        set
        {
            this._Uses = value;
            this.Update();
        }
    }

    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.InviteTracker.Cache = this.Parent.InviteTracker.Cache.Update(x => x.Code, this);
    }
}
