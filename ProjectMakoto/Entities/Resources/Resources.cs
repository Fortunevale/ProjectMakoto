// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
internal sealed class Resources
{
    public static readonly IReadOnlyList<Permissions> ProtectedPermissions = new List<Permissions>()
    {
        Permissions.Administrator,

        Permissions.MuteMembers,
        Permissions.DeafenMembers,
        Permissions.ModerateMembers,
        Permissions.KickMembers,
        Permissions.BanMembers,

        Permissions.ManageGuild,
        Permissions.ManageChannels,
        Permissions.ManageRoles,
        Permissions.ManageMessages,
        Permissions.ManageEvents,
        Permissions.ManageThreads,
        Permissions.ManageWebhooks,
        Permissions.ManageNicknames,

        Permissions.ViewAuditLog,
    };

    public static readonly string AbuseIpDbIcon = "https://cdn.discordapp.com/attachments/1005430437952356423/1021782030511517757/ezgif.com-gif-maker.png";
}