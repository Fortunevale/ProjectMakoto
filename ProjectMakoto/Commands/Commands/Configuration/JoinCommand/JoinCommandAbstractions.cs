// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.JoinCommand;

internal class JoinCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return  $"🌐 `Autoban Globally Banned Users`: {ctx.Bot.guilds[ctx.Guild.Id].Join.AutoBanGlobalBans.ToEmote(ctx.Bot)}\n" +
                $"👋 `Joinlog Channel              `: {(ctx.Bot.guilds[ctx.Guild.Id].Join.JoinlogChannelId != 0 ? $"<#{ctx.Bot.guilds[ctx.Guild.Id].Join.JoinlogChannelId}>" : false.ToEmote(ctx.Bot))}\n" +
                $"👤 `Role On Join                 `: {(ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId != 0 ? $"<@&{ctx.Bot.guilds[ctx.Guild.Id].Join.AutoAssignRoleId}>" : false.ToEmote(ctx.Bot))}\n" +
                $"👥 `Re-Apply Roles on Rejoin     `: {ctx.Bot.guilds[ctx.Guild.Id].Join.ReApplyRoles.ToEmote(ctx.Bot)}\n" +
                $"💬 `Re-Apply Nickname on Rejoin  `: {ctx.Bot.guilds[ctx.Guild.Id].Join.ReApplyNickname.ToEmote(ctx.Bot)}\n\n" +
                $"For security reasons, roles with any of the following permissions never get re-applied: {string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToPermissionString()}`"))}.\n\n" +
                $"In addition, if the user left the server 60+ days ago, neither roles nor nicknames will be re-applied.";
    }
}
