// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public static class UserExtensions
{
    public static bool IsTeamOwner(this DiscordMember member, Status _status)
    => (member as DiscordUser).IsTeamOwner(_status);

    public static bool IsTeamOwner(this DiscordUser user, Status _status)
    {
        return _status.TeamOwner == user.Id;
    }

    public static bool IsMaintenance(this DiscordMember member, Status _status)
        => (member as DiscordUser).IsMaintenance(_status);

    public static bool IsMaintenance(this DiscordUser user, Status _status)
    {
        return _status.TeamMembers.Contains(user.Id);
    }

    public static bool IsAdmin(this DiscordMember member, Status _status)
    {
        return (member.Roles.Any(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed || x.CheckPermission(Permissions.ManageGuild) == PermissionLevel.Allowed)) ||
            (member.IsMaintenance(_status)) ||
            member.IsOwner;
    }

    public static bool IsDJ(this DiscordMember member, Status _status)
    {
        return member.IsAdmin(_status) || member.Roles.Any(x => x.Name.ToLower() == "dj");
    }
}
