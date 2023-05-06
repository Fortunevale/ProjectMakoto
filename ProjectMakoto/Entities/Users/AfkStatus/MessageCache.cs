// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class MessageDetails
{
    private ulong _MessageId { get; set; } = 0;
    public ulong MessageId { get => _MessageId; set { _MessageId = value; } }



    private ulong _ChannelId { get; set; } = 0;
    public ulong ChannelId { get => _ChannelId; set { _ChannelId = value; } }



    private ulong _GuildId { get; set; } = 0;
    public ulong GuildId { get => _GuildId; set { _GuildId = value; } }



    private ulong _AuthorId { get; set; } = 0;
    public ulong AuthorId { get => _AuthorId; set { _AuthorId = value; } }
}
