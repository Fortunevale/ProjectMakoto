// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.JoinCommand;

internal sealed class JoinCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.Join;

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.Autoban, CommandKey.JoinLogChannel, CommandKey.Role, CommandKey.ReApplyRoles, CommandKey.ReApplyNickname);

        return $"{"🌐".UnicodeToEmoji()} `{CommandKey.Autoban.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.AutoBanGlobalBans.ToEmote(ctx.Bot)}\n" +
                $"{"👋".UnicodeToEmoji()} `{CommandKey.JoinLogChannel.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.JoinlogChannelId != 0 ? $"<#{ctx.DbGuild.Join.JoinlogChannelId}>" : false.ToEmote(ctx.Bot))}\n" +
                $"{"👤".UnicodeToEmoji()} `{CommandKey.Role.Get(ctx.DbUser).PadRight(pad)}`: {(ctx.DbGuild.Join.AutoAssignRoleId != 0 ? $"<@&{ctx.DbGuild.Join.AutoAssignRoleId}>" : false.ToEmote(ctx.Bot))}\n" +
                $"{"👥".UnicodeToEmoji()} `{CommandKey.ReApplyRoles.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyRoles.ToEmote(ctx.Bot)}\n" +
                $"{"💬".UnicodeToEmoji()} `{CommandKey.ReApplyNickname.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.Join.ReApplyNickname.ToEmote(ctx.Bot)}\n\n" +
                $"{CommandKey.SecurityNotice.Get(ctx.DbUser).Build(true, new TVar("Permissions", string.Join(", ", Resources.ProtectedPermissions.Select(x => $"`{x.ToTranslatedPermissionString(ctx.DbUser, ctx.Bot)}`"))))}\n\n" +
                $"{CommandKey.TimeNotice.Get(ctx.DbUser).Build()}";
    }
}
