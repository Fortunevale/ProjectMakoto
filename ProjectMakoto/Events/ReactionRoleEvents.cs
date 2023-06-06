// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class ReactionRoleEvents
{
    internal ReactionRoleEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        if (e.Guild is null || e.Channel is null || e.Channel.IsPrivate)
            return;

        if (this._bot.guilds[e.Guild.Id].ReactionRoles.Any(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName()))
        {
            var obj = this._bot.guilds[e.Guild.Id].ReactionRoles.First(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName());

            if (e.Guild.Roles.ContainsKey(obj.Value.RoleId) && e.User.Id != this._bot.discordClient.CurrentUser.Id)
                await (await e.User.ConvertToMember(e.Guild)).GrantRoleAsync(e.Guild.GetRole(obj.Value.RoleId));
        }
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        if (e.Guild == null || e.Channel.IsPrivate)
            return;

        if (this._bot.guilds[e.Guild.Id].ReactionRoles.Any<KeyValuePair<ulong, ReactionRoleEntry>>(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName()))
        {
            var obj = this._bot.guilds[e.Guild.Id].ReactionRoles.First<KeyValuePair<ulong, ReactionRoleEntry>>(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName());

            if (e.Guild.Roles.ContainsKey(obj.Value.RoleId) && e.User.Id != this._bot.discordClient.CurrentUser.Id)
            {
                DiscordMember member;

                try
                { member = await e.User.ConvertToMember(e.Guild); }
                catch (DisCatSharp.Exceptions.NotFoundException) { return; }
                await member.RevokeRoleAsync(e.Guild.GetRole(obj.Value.RoleId));
            }
        }
    }
}
