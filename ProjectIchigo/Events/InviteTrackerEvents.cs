﻿namespace ProjectIchigo.Events;

internal class InviteTrackerEvents
{
    internal InviteTrackerEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    private async Task UpdateCachedInvites(DiscordGuild guild)
    {
        _logger.LogDebug($"Fetching invites for {guild.Id}");

        var Invites = await guild.GetInvitesAsync();

        _bot._guilds.List[guild.Id].InviteTrackerSettings.Cache.Clear();

        for (int i = 0; i < Invites.Count; i++)
            _bot._guilds.List[guild.Id].InviteTrackerSettings.Cache.Add(new InviteTrackerCacheItem { Code = Invites[i].Code, CreatorId = Invites[i].Inviter.Id, Uses = Invites[i].Uses });

        _logger.LogDebug($"Fetched {_bot._guilds.List[guild.Id].InviteTrackerSettings.Cache.Count} invites for {guild.Id}");
    }



    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(e.Guild);
        }).Add(_bot._watcher);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(e.Guild);
        }).Add(_bot._watcher);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(e.Guild);
        }).Add(_bot._watcher);
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            _logger.LogDebug($"User {e.Member.Id} joined {e.Guild.Id}, trying to track invite used..");

            List<InviteTrackerCacheItem> InvitesBefore = new();
            List<InviteTrackerCacheItem> InvitesAfter = new();

            foreach (var b in _bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Cache)
                InvitesBefore.Add(b);

            await UpdateCachedInvites(e.Guild);

            foreach (var b in _bot._guilds.List[e.Guild.Id].InviteTrackerSettings.Cache)
                InvitesAfter.Add(b);

            foreach (var b in InvitesBefore)
            {
                if (!InvitesAfter.Any(x => x.Code == b.Code))
                    continue;

                if (InvitesAfter.First(x => x.Code == b.Code).Uses > b.Uses)
                {
                    _bot._guilds.List[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code = b.Code;
                    _bot._guilds.List[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId = b.CreatorId;
                    _logger.LogDebug($"User {e.Member.Id} joined {e.Guild.Id} with {b.Code} created by {b.CreatorId}");
                    return;
                }
            }

            _logger.LogDebug($"Could not track invite for user {e.Member.Id} who joined {e.Guild.Id}");
        }).Add(_bot._watcher);
    }
}
