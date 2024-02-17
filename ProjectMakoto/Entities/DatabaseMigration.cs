// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
internal class DatabaseMigration
{
    internal class ReactionRoles
    {
        public string UUID { get; set; }
        public ulong EmojiId { get; set; }
        public string EmojiName { get; set; }
        public ulong RoleId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
