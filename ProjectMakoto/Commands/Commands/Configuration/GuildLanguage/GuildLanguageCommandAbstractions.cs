// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.GuildLanguage;
internal sealed class GuildLanguageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"🗨 `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.GuildLanguage.Disclaimer)}`\n`{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.GuildLanguage.Response)}`: `{(ctx.DbGuild.OverrideLocale.IsNullOrWhiteSpace() ? (ctx.DbGuild.CurrentLocale.IsNullOrWhiteSpace() ? "en (Default)" : $"{ctx.DbGuild.CurrentLocale} (Discord)") : $"{ctx.DbGuild.OverrideLocale} (Override)")}`";
    }
}
