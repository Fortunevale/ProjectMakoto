// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class ReactionRoleEntry
{
    public string UUID = Guid.NewGuid().ToString();
    public ulong EmojiId { get; set; }
    public string EmojiName { get; set; }

    public DiscordEmoji GetEmoji(DiscordClient client)
    {
        return this.EmojiId == 0
            ? DiscordEmoji.FromName(client, $":{this.EmojiName.Remove(this.EmojiName.LastIndexOf(':'), this.EmojiName.Length - this.EmojiName.LastIndexOf(':'))}:")
            : DiscordEmoji.FromGuildEmote(client, this.EmojiId);
    }

    public ulong RoleId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong MessageId { get; set; }
}
