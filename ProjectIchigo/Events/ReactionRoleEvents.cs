namespace ProjectIchigo.Events;

internal class ReactionRoleEvents
{
    internal ReactionRoleEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate)
                return;

            if (!_bot._guilds.ContainsKey(e.Guild.Id))
                _bot._guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            if (_bot._guilds[e.Guild.Id].ReactionRoles.Any(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName()))
            {
                var obj = _bot._guilds[ e.Guild.Id ].ReactionRoles.First(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName());

                if (e.Guild.Roles.ContainsKey(obj.Value.RoleId) && e.User.Id != _bot.discordClient.CurrentUser.Id)
                    await (await e.User.ConvertToMember(e.Guild)).GrantRoleAsync(e.Guild.GetRole(obj.Value.RoleId));
            }
        }).Add(_bot._watcher);
    }

    internal async Task MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (e.Guild == null || e.Channel.IsPrivate)
                return;

            if (!_bot._guilds.ContainsKey(e.Guild.Id))
                _bot._guilds.Add(e.Guild.Id, new Guild(e.Guild.Id));

            if (_bot._guilds[ e.Guild.Id ].ReactionRoles.Any(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName()))
            {
                var obj = _bot._guilds[ e.Guild.Id ].ReactionRoles.First(x => x.Key == e.Message.Id && x.Value.EmojiName == e.Emoji.GetUniqueDiscordName());

                if (e.Guild.Roles.ContainsKey(obj.Value.RoleId) && e.User.Id != _bot.discordClient.CurrentUser.Id)
                    await (await e.User.ConvertToMember(e.Guild)).RevokeRoleAsync(e.Guild.GetRole(obj.Value.RoleId));
            }
        }).Add(_bot._watcher);
    }
}
