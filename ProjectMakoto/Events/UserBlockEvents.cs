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
        Permissions.Administrator,
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
            if (e.User.IsBot)
                return;

            var joiningMember = await e.User.ConvertToMember(e.Guild);
            var membersWithBlocks = e.After.Channel.Users.Where(x => x.Id != joiningMember.Id).Where(x => this.Bot.Users[x.Id].BlockedUsers.Contains(e.User.Id));
            var blockedMembers = e.After.Channel.Users.Where(x => x.Id != joiningMember.Id).Where(x => e.User.GetDbEntry(this.Bot).BlockedUsers.Contains(x.Id));

            var memberWithBlocksHighestRole = membersWithBlocks?.MaxBy(x => GetModerationStatus(x));
            var blockedMemberHighestRole = blockedMembers?.MaxBy(x => GetModerationStatus(x));
            int GetModerationStatus(DiscordMember? member)
            {
                var i = -1;

                if (member is not null && member.Permissions.HasAnyPermission(this.ModerationPermissions))
                    i = member.GetRoleHighestPosition();
                return i;
            }

            if (membersWithBlocks?.IsNotNullAndNotEmpty() ?? false)
            {
                if (GetModerationStatus(joiningMember) > GetModerationStatus(memberWithBlocksHighestRole))
                    return;

                _ = joiningMember.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(this.t.Commands.Social.BlockedByVictim.Get(joiningMember.GetDbEntry(this.Bot))
                        .Build(true, new TVar("User", membersWithBlocks.First().Mention)))
                    .AsError(new SharedCommandContext()
                    {
                        Bot = this.Bot,
                        User = e.User,
                        Client = sender,
                        DbUser = e.User.GetDbEntry(this.Bot),
                    }).WithFooter());

                if (e.Before?.Channel is not null)
                    await joiningMember.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await joiningMember.DisconnectFromVoiceAsync();
            }
            else if (this.Bot.Users[e.User.Id].BlockedUsers.Any(blockedId => e.Channel.Users.Any(user => user.Id == blockedId)))
            {
                if (GetModerationStatus(joiningMember) > GetModerationStatus(blockedMemberHighestRole))
                    return;

                _ = joiningMember.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(this.t.Commands.Social.BlockedVictim.Get(joiningMember.GetDbEntry(this.Bot))
                        .Build(true, new TVar("User", $"<@{this.Bot.Users[e.User.Id].BlockedUsers.First(blockedId => e.Channel.Users.Any(user => user.Id == blockedId))}>")))
                    .AsError(new SharedCommandContext()
                    {
                        Bot = this.Bot,
                        User = e.User,
                        Client = sender,
                        DbUser = e.User.GetDbEntry(this.Bot),
                    }).WithFooter());

                if (e.Before?.Channel is not null)
                    await joiningMember.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await joiningMember.DisconnectFromVoiceAsync();
            }
        }
    }
}
