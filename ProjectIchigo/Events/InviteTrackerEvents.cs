namespace ProjectIchigo.Events;

internal class InviteTrackerEvents
{
    internal InviteTrackerEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    public static async Task UpdateCachedInvites(Bot _bot, DiscordGuild guild)
    {
        _logger.LogDebug($"Fetching invites for {guild.Id}");

        var Invites = await guild.GetInvitesAsync();

        _bot.guilds[guild.Id].InviteTrackerSettings.Cache = Invites.Select(x => new InviteTrackerCacheItem { Code = x.Code, CreatorId = x.Inviter?.Id ?? 0, Uses = x.Uses }).ToList();

        _logger.LogDebug($"Fetched {_bot.guilds[guild.Id].InviteTrackerSettings.Cache.Count} invites for {guild.Id}");
    }



    internal async Task GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            await UpdateCachedInvites(_bot, e.Guild);
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].InviteTrackerSettings.Enabled)
                return;

            _logger.LogDebug($"User {e.Member.Id} joined {e.Guild.Id}, trying to track invite used..");

            List<InviteTrackerCacheItem> InvitesBefore = new();
            List<InviteTrackerCacheItem> InvitesAfter = new();

            foreach (var b in _bot.guilds[e.Guild.Id].InviteTrackerSettings.Cache)
                InvitesBefore.Add(b);

            await UpdateCachedInvites(_bot, e.Guild);

            foreach (var b in _bot.guilds[e.Guild.Id].InviteTrackerSettings.Cache)
                InvitesAfter.Add(b);

            foreach (var b in InvitesBefore)
            {
                if (!InvitesAfter.Any(x => x.Code == b.Code))
                {
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code = b.Code;
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId = b.CreatorId;
                    _logger.LogDebug($"User {e.Member.Id} joined {e.Guild.Id} with now deleted {b.Code} created by {b.CreatorId}");
                    return;
                }

                if (InvitesAfter.First(x => x.Code == b.Code).Uses > b.Uses)
                {
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.Code = b.Code;
                    _bot.guilds[e.Guild.Id].Members[e.Member.Id].InviteTracker.UserId = b.CreatorId;
                    _logger.LogDebug($"User {e.Member.Id} joined {e.Guild.Id} with {b.Code} created by {b.CreatorId}");
                    return;
                }
            }

            _logger.LogDebug($"Could not track invite for user {e.Member.Id} who joined {e.Guild.Id}");
        }).Add(_bot.watcher);
    }
}
