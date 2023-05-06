// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal class EmojiEntry
{
    public string Name { get; set; }
    public EmojiType Type { get; set; }
    public string Path { get; set; }
    public bool Animated { get; set; }
}
