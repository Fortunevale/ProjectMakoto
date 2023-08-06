// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Members;

namespace ProjectMakoto.Events;

internal sealed class GenericGuildEvents : RequiresTranslation
{
    public GenericGuildEvents(Bot bot) : base(bot)
    {
    }

    internal async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));

        if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate == DateTime.UnixEpoch)
            this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].FirstJoinDate = e.Member.JoinedAt.UtcDateTime;

        if (this.Bot.Guilds[e.Guild.Id].Join.ReApplyNickname)
            if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays < 60)
                _ = e.Member.ModifyAsync(x => x.Nickname = this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname).Add(this.Bot);

        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UnixEpoch;

        if (!this.Bot.Guilds[e.Guild.Id].Join.ReApplyRoles)
            return;

        if (e.Member.IsBot)
            return;

        if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate.ToUniversalTime().GetTimespanSince().TotalDays > 60)
            return;

        if (this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Count > 0)
        {
            var HighestRoleOnBot = (await e.Guild.GetMemberAsync(sender.CurrentUser.Id)).Roles.OrderByDescending(x => x.Position).First().Position;

            List<MemberRole> disallowedRoles = new();
            List<MemberRole> deletedRoles = new();

            List<DiscordRole> rolesToApply = new();

            foreach (var b in this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles)
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
                _ = e.Member.ReplaceRolesAsync(rolesToApply, "Role Backup").Add(this.Bot);
        }
    }

    internal async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));

        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].LastLeaveDate = DateTime.UtcNow;
    }

    internal async Task GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));
        await Task.Delay(2000);

        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles = e.Member.Roles.Select(x => new MemberRole
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList();

        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = e.Member.Nickname;
    }

    internal async Task GuildMemberBanned(DiscordClient sender, GuildBanAddEventArgs e)
    {
        if (!this.Bot.Guilds[e.Guild.Id].Members.ContainsKey(e.Member.Id))
            this.Bot.Guilds[e.Guild.Id].Members.Add(e.Member.Id, new(this.Bot, this.Bot.Guilds[e.Guild.Id], e.Member.Id));

        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].MemberRoles.Clear();
        this.Bot.Guilds[e.Guild.Id].Members[e.Member.Id].SavedNickname = "";
    }
}
