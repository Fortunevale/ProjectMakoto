namespace Project_Ichigo.Events;

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
            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (!_bot._guilds.Servers[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot._guilds.Servers[e.Guild.Id].Members.Add(e.Member.Id, new());


            if (_bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].FirstJoinDate == DateTime.UnixEpoch)
                _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].FirstJoinDate = e.Member.JoinedAt.UtcDateTime;

            if (_bot._guilds.Servers[e.Guild.Id].JoinSettings.ReApplyNickname)
                if (_bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays < 60)
                    e.Member.ModifyAsync(x => x.Nickname = _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].SavedNickname).Add(_bot._watcher);

            Task task = Task.Run(async () =>
            {
                if (!_bot._guilds.Servers[e.Guild.Id].JoinSettings.ReApplyRoles)
                    return;

                if (e.Member.IsBot)
                    return;

                if (_bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays > 60)
                    return;

                if (_bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].MemberRoles.Count > 0)
                {
                    var HighestRoleOnBot = (await e.Guild.GetMemberAsync(sender.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

                    List<MembersRole> disallowedRoles = new();
                    List<MembersRole> deletedRoles = new();

                    List<DiscordRole> rolesToApply = new();

                    foreach (var b in _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].MemberRoles)
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
                        e.Member.ReplaceRolesAsync(rolesToApply, "Role Backup").Add(_bot._watcher);
                }
            });
            task.Add(_bot._watcher);

            try
            {
                await task.WaitAsync(TimeSpan.FromSeconds(60));
            }
            catch { }

            _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UnixEpoch;
        }).Add(_bot._watcher);
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        Task.Run(async () =>
        {
            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (!_bot._guilds.Servers[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot._guilds.Servers[e.Guild.Id].Members.Add(e.Member.Id, new());

            _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UtcNow;
        }).Add(_bot._watcher);
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        Task.Run(async () =>
        {
            await Task.Delay(5000);

            if (!_bot._guilds.Servers.ContainsKey(e.Guild.Id))
                _bot._guilds.Servers.Add(e.Guild.Id, new ServerInfo.ServerSettings());

            if (!_bot._guilds.Servers[e.Guild.Id].Members.ContainsKey(e.Member.Id))
                _bot._guilds.Servers[e.Guild.Id].Members.Add(e.Member.Id, new());

            _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].MemberRoles = e.Member.Roles.Select(x => new MembersRole
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList();

            _bot._guilds.Servers[e.Guild.Id].Members[e.Member.Id].SavedNickname = e.Member.Nickname;
        }).Add(_bot._watcher);
    }
}
