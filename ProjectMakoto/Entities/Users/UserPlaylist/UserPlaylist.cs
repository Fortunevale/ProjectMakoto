// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class UserPlaylist
{
    public string PlaylistId { get; set; } = Guid.NewGuid().ToString();

    private string _PlaylistName { get; set; } = "";

    [JsonProperty(Required = Required.Always)]
    public string PlaylistName { get => _PlaylistName; set { _PlaylistName = value.TruncateWithIndication(256); } }

    private string _PlaylistColor { get; set; } = "#FFFFFF";
    public string PlaylistColor { get => _PlaylistColor; set { _PlaylistColor = value.Truncate(7).IsValidHexColor(); } }

    private string _PlaylistThumbnail { get; set; } = "";
    public string PlaylistThumbnail { get => _PlaylistThumbnail; set { _PlaylistThumbnail = value.Truncate(2048); } }

    [JsonProperty(Required = Required.Always)]
    public List<PlaylistEntry> List { get; set; } = new();
}
