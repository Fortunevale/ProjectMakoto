// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users;

public sealed class UserPlaylist
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public User Parent { get; set; }

    public string PlaylistId { get; set; } = Guid.NewGuid().ToString();

    private string _PlaylistName { get; set; } = "";

    [JsonProperty(Required = Required.Always)]
    public string PlaylistName
    {
        get => this._PlaylistName;
        set
        {
            this._PlaylistName = value.TruncateWithIndication(256);
            this.Update();
        }
    }

    private string _PlaylistColor { get; set; } = "#FFFFFF";
    public string PlaylistColor
    {
        get => this._PlaylistColor;
        set
        {
            this._PlaylistColor = value.Truncate(7).IsValidHexColor();
            this.Update();
        }
    }

    private string _PlaylistThumbnail { get; set; } = "";
    public string PlaylistThumbnail
    {
        get => this._PlaylistThumbnail;
        set
        {
            this._PlaylistThumbnail = value.Truncate(2048);
            this.Update();
        }
    }

    private PlaylistEntry[] _List = Array.Empty<PlaylistEntry>();

    [JsonProperty(Required = Required.Always)]
    public PlaylistEntry[] List
    {
        get => this._List;
        set
        {
            this._List = value.Take(250).ToArray();
            this.Update();
        }
    }

    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.UserPlaylists = this.Parent.UserPlaylists.Update(x => x.PlaylistId, this);
    }
}