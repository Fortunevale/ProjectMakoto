namespace ProjectIchigo.Commands.ReactionRolesCommand;

internal class ReactionRolesCommandAbstractions
{
    internal static async Task<Dictionary<ulong, DiscordMessage>> CheckForInvalid(SharedCommandContext ctx)
    {
        if (await ctx.Bot._users[ctx.Member.Id].Cooldown.WaitForHeavy(ctx.Client, ctx))
            return new();

        Dictionary<ulong, DiscordMessage> messageCache = new();

        foreach (var b in ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.ToList())
        {
            if (!ctx.Guild.Channels.ContainsKey(b.Value.ChannelId))
            {
                ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                continue;
            }

            if (!ctx.Guild.Roles.ContainsKey(b.Value.RoleId))
            {
                ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
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

                    ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }
                catch (DisCatSharp.Exceptions.UnauthorizedException)
                {
                    messageCache.Add(b.Key, null);

                    ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    continue;
                }
            }

            if (messageCache[b.Key] == null)
            {
                ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                continue;
            }

            var msg = messageCache[b.Key];

            if (!msg.Reactions.Any(x => x.Emoji.Id == b.Value.EmojiId && x.Emoji.GetUniqueDiscordName() == b.Value.EmojiName && x.IsMe))
            {
                _ = msg.CreateReactionAsync(b.Value.GetEmoji(ctx.Client)).ContinueWith(x =>
                {
                    if (x.IsFaulted)
                    {
                        ctx.Bot._guilds[ctx.Guild.Id].ReactionRoles.Remove(b);
                    }
                });
                continue;
            }
        }

        return messageCache;
    }
}
