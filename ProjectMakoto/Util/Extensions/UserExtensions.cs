// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

internal static class UserExtensions
{
    internal static bool IsTeamOwner(this DiscordMember member, Status _status)
    => (member as DiscordUser).IsTeamOwner(_status);

    internal static bool IsTeamOwner(this DiscordUser user, Status _status)
    {
        if (_status.TeamOwner == user.Id)
            return true;

        return false;
    }

    internal static bool IsMaintenance(this DiscordMember member, Status _status) 
        => (member as DiscordUser).IsMaintenance(_status);

    internal static bool IsMaintenance(this DiscordUser user, Status _status)
    {
        if (_status.TeamMembers.Contains(user.Id))
            return true;

        return false;
    }

    internal static bool IsAdmin(this DiscordMember member, Status _status)
    {
        if ((member.Roles.Any(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed || x.CheckPermission(Permissions.ManageGuild) == PermissionLevel.Allowed)) ||
            (member.IsMaintenance(_status)) ||
            member.IsOwner)
            return true;

        return false;
    }

    internal static bool IsDJ(this DiscordMember member, Status _status)
    {
        if (member.IsAdmin(_status) || member.Roles.Any(x => x.Name.ToLower() == "dj"))
            return true;

        return false;
    }
}
