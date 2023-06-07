// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class ChannelPromptConfiguration
{
    public ChannelConfig CreateChannelOption { get; set; } = null;

    public string? DisableOption { get; set; } = null;

    public sealed class ChannelConfig
    {
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
    }
}
