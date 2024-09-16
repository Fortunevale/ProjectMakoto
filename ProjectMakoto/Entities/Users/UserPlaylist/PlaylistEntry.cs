// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Users.Legacy;

public sealed class PlaylistEntry
{
    private string _Title { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Title { get => this._Title; set => this._Title = value.TruncateWithIndication(100); }

    private TimeSpan? _Length { get; set; }
    public TimeSpan? Length { get => this._Length; set => this._Length = value; }

    private string _Url { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Url { get => this._Url; set => this._Url = value.TruncateWithIndication(2048); }

    public DateTime AddedTime { get; set; } = DateTime.UtcNow;
}
