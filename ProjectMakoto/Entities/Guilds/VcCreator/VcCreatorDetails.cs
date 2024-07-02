// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class VcCreatorDetails
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public VcCreatorSettings Parent { get; set; }

    private ulong _ChannelId { get; set; }
    public ulong ChannelId
    {
        get => this._ChannelId;
        set
        {
            this._ChannelId = value;
            this.Update();
        }
    }

    private ulong _OwnerId { get; set; }
    public ulong OwnerId
    {
        get => this._OwnerId;
        set
        {
            this._OwnerId = value;
            this.Update();
        }
    }

    private ulong[] _BannedUsers { get; set; } = Array.Empty<ulong>();
    public ulong[] BannedUsers
    {
        get => this._BannedUsers;
        set
        {
            this._BannedUsers = value;
            this.Update();
        }
    }

    private DateTime _LastRename { get; set; } = DateTime.MinValue;
    public DateTime LastRename
    {
        get => this._LastRename;
        set
        {
            this._LastRename = value;
            this.Update();
        }
    }

    [JsonIgnore]
    public bool EventsRegistered { get; set; } = false;

    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.CreatedChannels = this.Parent.CreatedChannels.Update(x => x.ChannelId.ToString(), this);
    }
}
