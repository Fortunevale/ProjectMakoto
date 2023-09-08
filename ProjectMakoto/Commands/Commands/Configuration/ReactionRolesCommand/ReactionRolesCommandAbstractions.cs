// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.ReactionRolesCommand;

internal static class ReactionRolesCommandAbstractions
{
    internal static async Task<Dictionary<ulong, DiscordMessage>> CheckForInvalid(SharedCommandContext ctx)
    {
        if (await ctx.DbUser.Cooldown.WaitForHeavy(ctx))
            return new();

        Dictionary<ulong, DiscordMessage> messageCache = new();

        foreach (var b in ctx.DbGuild.ReactionRoles.ToList())
        {
            if (!ctx.Guild.Channels.ContainsKey(b.ChannelId))
            {
                ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                continue;
            }

            if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
            {
                ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                continue;
            }

            var channel = ctx.Guild.GetChannel(b.ChannelId);

            if (!messageCache.ContainsKey(b.MessageId))
            {
                try
                {
                    var requested_msg = await channel.GetMessageAsync(b.MessageId);
                    messageCache.Add(b.MessageId, requested_msg);
                }
                catch (DisCatSharp.Exceptions.NotFoundException)
                {
                    messageCache.Add(b.MessageId, null);

                    ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                    continue;
                }
                catch (DisCatSharp.Exceptions.UnauthorizedException)
                {
                    messageCache.Add(b.MessageId, null);

                    ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                    continue;
                }
            }

            if (messageCache[b.MessageId] == null)
            {
                ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                continue;
            }

            var msg = messageCache[b.MessageId  ];

            if (!msg.Reactions.Any(x => x.Emoji.Id == b.EmojiId && x.Emoji.GetUniqueDiscordName() == b.EmojiName && x.IsMe))
            {
                _ = msg.CreateReactionAsync(b.GetEmoji(ctx.Client)).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        ctx.DbGuild.ReactionRoles = ctx.DbGuild.ReactionRoles.Remove(x => x.MessageId.ToString(), b);
                    }
                });
                continue;
            }
        }

        return messageCache;
    }
}
