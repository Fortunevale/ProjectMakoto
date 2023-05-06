﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal class ReactionRolesCommandAbstractions
{
    internal static async Task<Dictionary<ulong, DiscordMessage>> CheckForInvalid(SharedCommandContext ctx)
    {
        if (await ctx.Bot.users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx))
            return new();

        Dictionary<ulong, DiscordMessage> messageCache = new();

        foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.ToList())
        {
            if (!ctx.Guild.Channels.ContainsKey(b.Value.ChannelId))
            {
                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                continue;
            }

            if (!ctx.Guild.Roles.ContainsKey(b.Value.RoleId))
            {
                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                continue;
            }

            var channel = ctx.Guild.GetChannel(b.Value.ChannelId);

            if (!messageCache.ContainsKey(b.Key))
            {
                try
                {
                    var requested_msg = await channel.GetMessageAsync(b.Key);
                    messageCache.Add(b.Key, requested_msg);
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    messageCache.Add(b.Key, null);

                    ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }
                catch (DisCatSharp.Exceptions.UnauthorizedException)
                {
                    messageCache.Add(b.Key, null);

                    ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }
            }

            if (messageCache[b.Key] == null)
            {
                ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                continue;
            }

            var msg = messageCache[b.Key];

            if (!msg.Reactions.Any(x => x.Emoji.Id == b.Value.EmojiId && x.Emoji.GetUniqueDiscordName() == b.Value.EmojiName && x.IsMe))
            {
                _ = msg.CreateReactionAsync(b.Value.GetEmoji(ctx.Client)).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        ctx.Bot.guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    }
                });
                continue;
            }
        }

        return messageCache;
    }
}
