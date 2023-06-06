// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class EmojiEntry
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DiscordEmoji Emoji { get; set; }
    public StickerFormat StickerFormat { get; set; }
    public EmojiType EntryType { get; set; }
    public bool Animated { get; set; }

    public data Data { get; set; } = new();
    public sealed class data
    {
        public string Name { get; set; }
        public Stream Stream { get; set; } = new MemoryStream();
    }
}
