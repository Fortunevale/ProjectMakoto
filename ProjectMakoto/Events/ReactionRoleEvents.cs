// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class ReactionRoleEvents(Bot bot) : RequiresTranslation(bot)
{
    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Channel.IsPrivate)
            return;

        if (this.Bot.Guilds[e.Guild.Id].ReactionRoles.Any(x => x.MessageId == e.Message.Id && x.EmojiName == e.Emoji.GetUniqueDiscordName()))
        {
            var obj = this.Bot.Guilds[e.Guild.Id].ReactionRoles.First(x => x.MessageId == e.Message.Id && x.EmojiName == e.Emoji.GetUniqueDiscordName());

            if (e.Guild.Roles.ContainsKey(obj.RoleId) && e.User.Id != this.Bot.DiscordClient.CurrentUser.Id)
                await (await e.User.ConvertToMember(e.Guild)).GrantRoleAsync(e.Guild.GetRole(obj.RoleId));
        }
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        if (e.Guild == null || e.Channel.IsPrivate)
            return;

        if (this.Bot.Guilds[e.Guild.Id].ReactionRoles.Any(x => x.MessageId == e.Message.Id && x.EmojiName == e.Emoji.GetUniqueDiscordName()))
        {
            var obj = this.Bot.Guilds[e.Guild.Id].ReactionRoles.First(x => x.MessageId == e.Message.Id && x.EmojiName == e.Emoji.GetUniqueDiscordName());

            if (e.Guild.Roles.ContainsKey(obj.RoleId) && e.User.Id != this.Bot.DiscordClient.CurrentUser.Id)
            {
                DiscordMember member;

                try
                { member = await e.User.ConvertToMember(e.Guild); }
                catch (DisCatSharp.Exceptions.NotFoundException) { return; }
                await member.RevokeRoleAsync(e.Guild.GetRole(obj.RoleId));
            }
        }
    }
}
