// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class CrosspostRatelimit
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public Guild Parent { get; set; }

    private ulong _Id { get; set; }
    public ulong Id
    {
        get => this._Id;
        set
        {
            this._Id = value;
            this.Update();
        }
    }

    private DateTime _FirstPost { get; set; } = DateTime.MinValue;
    public DateTime FirstPost
    {
        get => this._FirstPost;
        set
        {
            this._FirstPost = value;
            this.Update();
        }
    }

    private int _PostsRemaining { get; set; } = 0;
    public int PostsRemaining
    {
        get => this._PostsRemaining;
        set
        {
            this._PostsRemaining = value;
            this.Update();
        }
    }

    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.Crosspost.CrosspostRatelimits = this.Parent.Crosspost.CrosspostRatelimits.Update(x => x.Id.ToString(), this);
    }
}
