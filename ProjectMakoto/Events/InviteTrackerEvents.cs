// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal class InviteTrackerEvents
{
    internal InviteTrackerEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    public static async Task UpdateCachedInvites(Bot _bot, DiscordGuild guild)
    {
        _logger.LogDebug("Fetching invites for {Guild}", guild.Id);

        var Invites = await guild.GetInvitesAsync();

        _bot.guilds[guild.Id].InviteTracker.Cache = Invites.Select(x => new InviteTrackerCacheItem { Code = x.Code, CreatorId = x.Inviter?.Id ?? 0, Uses = x.Uses }).ToList();

        _logger.LogDebug("Fetched {Count} invites for {Guild}", _bot.guilds[guild.Id].InviteTracker.Cache.Count, guild.Id);
    }



    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTracker.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTracker.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTracker.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTracker.Enabled)
                return;

            _logger.LogDebug("User '{User}' joined '{Guild}', trying to track invite used..", e.Member.Id, e.Guild.Id);

            List<InviteTrackerCacheItem> InvitesBefore = new();
            List<InviteTrackerCacheItem> InvitesAfter = new();

            foreach (var b in _bot.guilds[e.Guild.Id].InviteTracker.Cache)
                InvitesBefore.Add(b);

            await UpdateCachedInvites(_bot, e.Guild);

            foreach (var b in _bot.guilds[e.Guild.Id].InviteTracker.Cache)
                InvitesAfter.Add(b);

            foreach (var b in InvitesBefore)
            {
                if (!InvitesAfter.Any(x => x.Code == b.Code))
                {
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code = b.Code;
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId = b.CreatorId;
                    _logger.LogDebug("User '{User}' joined '{Guild}' with now deleted '{Code}' created by '{Creator}'", e.Member.Id, e.Guild.Id, b.Code, b.CreatorId);
                    return;
                }

                if (InvitesAfter.First(x => x.Code == b.Code).Uses > b.Uses)
                {
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code = b.Code;
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId = b.CreatorId;
                    _logger.LogDebug("User '{User}' joined '{Guild}' with '{Code}' created by '{Creator}'", e.Member.Id, e.Guild.Id, b.Code, b.CreatorId);
                    return;
                }
            }

            _logger.LogDebug("Could not track invite for user '{User}' who joined '{Guild}'", e.Member.Id, e.Guild.Id);
        }).Add(_bot.watcher);
    }
}
