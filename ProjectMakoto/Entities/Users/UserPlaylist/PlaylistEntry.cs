// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class PlaylistEntry
{
    private string _Title { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Title { get => _Title; set => _Title = value.TruncateWithIndication(100); }
    
    private TimeSpan? _Length { get; set; }
    public TimeSpan? Length { get => _Length; set => _Length = value; }

    private string _Url { get; set; }
    [JsonProperty(Required = Required.Always)]
    public string Url { get => _Url; set => _Url = value.TruncateWithIndication(2048); }

    public DateTime AddedTime { get; set; } = DateTime.UtcNow;
}
