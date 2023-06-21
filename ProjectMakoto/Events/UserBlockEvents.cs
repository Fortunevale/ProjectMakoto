// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;
internal sealed class UserBlockEvents : RequiresTranslation
{
    internal UserBlockEvents(Bot bot) : base(bot)
    {
    }

    internal readonly Permissions[] ModerationPermissions =
    {
        Permissions.MuteMembers,
        Permissions.DeafenMembers,
        Permissions.ModerateMembers,
        Permissions.KickMembers,
        Permissions.BanMembers,
    };

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (e.After.Channel != null && !e.Channel.IsPrivate)
        {
            var member = await e.User.ConvertToMember(e.Guild);
            var memberBlocks = e.After.Channel.Users.Where(x => this.Bot.Users[x.Id].BlockedUsers.Contains(e.User.Id));

            if (memberBlocks?.IsNotNullAndNotEmpty() ?? false)
            {
                if (member.Permissions.HasAnyPermission(this.ModerationPermissions))
                    if (!memberBlocks.Any(x => (x.Permissions.HasAnyPermission(this.ModerationPermissions))))
                        return;

                if (e.Before?.Channel is not null)
                    await member.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await member.DisconnectFromVoiceAsync();
            }
            else if (this.Bot.Users[e.User.Id].BlockedUsers.Any(blockedId => e.Channel.Users.Any(user => user.Id == blockedId)))
            {
                if (member.Permissions.HasAnyPermission(this.ModerationPermissions))
                    if (!e.Channel.Users.Where(x => this.Bot.Users[e.User.Id].BlockedUsers.Contains(x.Id)).Any(user => user.Permissions.HasAnyPermission(this.ModerationPermissions)))
                        return;

                if (e.Before?.Channel is not null)
                    await member.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await member.DisconnectFromVoiceAsync();
            }
        }
    }
}
