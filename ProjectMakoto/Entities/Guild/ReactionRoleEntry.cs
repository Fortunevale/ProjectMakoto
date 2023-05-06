// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class ReactionRoleEntry
{
    public string UUID = Guid.NewGuid().ToString();
    public ulong EmojiId { get; set; }
    public string EmojiName { get; set; }

    public DiscordEmoji GetEmoji(DiscordClient client)
    {
        if (EmojiId == 0)
            return DiscordEmoji.FromName(client, $":{EmojiName.Remove(EmojiName.LastIndexOf(":"), EmojiName.Length - EmojiName.LastIndexOf(":"))}:");

        return DiscordEmoji.FromGuildEmote(client, EmojiId);
    }

    public ulong RoleId { get; set; }
    public ulong ChannelId { get; set; }
}
