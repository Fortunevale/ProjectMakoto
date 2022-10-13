namespace ProjectIchigo.Events;

internal class GenericGuildEvents
{
    internal GenericGuildEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    internal Bot _bot { get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(_bot.guilds[e.Guild.Id], e.Member.Id));


            if (_bot.guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate == DateTime.UnixEpoch)
                _bot.guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate = e.Member.JoinedAt.UtcDateTime;

            if (_bot.guilds[e.Guild.Id].Join.ReApplyNickname)
                if (_bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays < 60)
                    e.Member.ModifyAsync(x => x.Nickname = _bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname).Add(_bot.watcher);

            Task task = Task.Run(async () =>
            {
                if (!_bot.guilds[e.Guild.Id].Join.ReApplyRoles)
                    return;

                if (e.Member.IsBot)
                    return;

                if (_bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays > 60)
                    return;

                if (_bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Count > 0)
                {
                    var HighestRoleOnBot = (await e.Guild.GetMemberAsync(sender.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                    List<MemberRole> disallowedRoles = new();
                    List<MemberRole> deletedRoles = new();

                    List<DiscordRole> rolesToApply = new();

                    foreach (var b in _bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles)
                    {
                        if (!e.Guild.Roles.ContainsKey(b.Id))
                        {
                            deletedRoles.Add(b);
                            continue;
                        }

                        var role = e.Guild.GetRole(b.Id);

                        foreach (var perm in Resources.ProtectedPermissions)
                            if (role.CheckPermission(perm) == PermissionLevel.Allowed)
                            {
                                disallowedRoles.Add(b);
                                continue;
                            }

                        if (role.IsManaged || role.Position >= HighestRoleOnBot)
                        {
                            disallowedRoles.Add(b);
                            continue;
                        }

                        rolesToApply.Add(role);
                    }

                    if (rolesToApply.Count > 0)
                        e.Member.ReplaceRolesAsync(rolesToApply, "Role Backup").Add(_bot.watcher);
                }
            });
            task.Add(_bot.watcher);

            try
            {
                await task.WaitAsync(TimeSpan.FromSeconds(60));
            }
            catch { }

            _bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UnixEpoch;
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(_bot.guilds[e.Guild.Id], e.Member.Id));

            _bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UtcNow;
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            await Task.Delay(5000);

            if (!_bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(_bot.guilds[e.Guild.Id], e.Member.Id));

            _bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles = e.Member.Roles.Select(x => new MemberRole
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList();

            _bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = e.Member.Nickname;
        }).Add(_bot.watcher);
    }

    internal async Task GuildMemberBanned(DiscordClient sender, GuildBanAddEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(_bot.guilds[e.Guild.Id], e.Member.Id));

            _bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Clear();
            _bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = "";
        }).Add(_bot.watcher);
    }
}
