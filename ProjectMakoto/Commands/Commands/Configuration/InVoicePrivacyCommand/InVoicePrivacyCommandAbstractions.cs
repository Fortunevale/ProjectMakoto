// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.InVoicePrivacyCommand;

internal sealed class InVoicePrivacyCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.LoadedTranslations.Commands.Config.InVoicePrivacy;

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.ClearMessagesOnLeave, CommandKey.SetPermissions);

        return $"{"🗑".UnicodeToEmoji()} `{CommandKey.ClearMessagesOnLeave.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.InVoiceTextPrivacy.ClearTextEnabled.ToEmote(ctx.Bot)}\n" +
               $"{"📋".UnicodeToEmoji()} `{CommandKey.SetPermissions.Get(ctx.DbUser).PadRight(pad)}`: {ctx.DbGuild.InVoiceTextPrivacy.SetPermissionsEnabled.ToEmote(ctx.Bot)}";
    }
}
