namespace ProjectIchigo.Util;

internal static class UserExtensions
{
    internal static bool IsMaintenance(this DiscordMember member, Status _status) => (member as DiscordUser).IsMaintenance(_status);

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
