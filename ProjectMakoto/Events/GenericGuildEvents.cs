// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class GenericGuildEvents
{
    internal GenericGuildEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    internal Bot _bot { get; set; }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (!this._bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this._bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this._bot.guilds[e.Guild.Id], e.Member.Id));


        if (this._bot.guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate == DateTime.UnixEpoch)
            this._bot.guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate = e.Member.JoinedAt.UtcDateTime;

        if (this._bot.guilds[e.Guild.Id].Join.ReApplyNickname)
            if (this._bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays < 60)
                e.Member.ModifyAsync(x => x.Nickname = this._bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname).Add(this._bot);

        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UnixEpoch;

        if (!this._bot.guilds[e.Guild.Id].Join.ReApplyRoles)
            return;

        if (e.Member.IsBot)
            return;

        if (this._bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays > 60)
            return;

        if (this._bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Count > 0)
        {
            var HighestRoleOnBot = (await e.Guild.GetMemberAsync(sender.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

            List<MemberRole> disallowedRoles = new();
            List<MemberRole> deletedRoles = new();

            List<DiscordRole> rolesToApply = new();

            foreach (var b in this._bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles)
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
                e.Member.ReplaceRolesAsync(rolesToApply, "Role Backup").Add(this._bot);
        }
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (!this._bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this._bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this._bot.guilds[e.Guild.Id], e.Member.Id));

        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UtcNow;
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        await Task.Delay(2000);

        if (!this._bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this._bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this._bot.guilds[e.Guild.Id], e.Member.Id));

        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles = e.Member.Roles.Select(x => new MemberRole
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList();

        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = e.Member.Nickname;
    }

    internal async Task GuildMemberBanned(DiscordClient sender, GuildBanAddEventArgs e)
    {
        if (!this._bot.guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this._bot.guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this._bot.guilds[e.Guild.Id], e.Member.Id));

        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Clear();
        this._bot.guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = "";
    }
}
