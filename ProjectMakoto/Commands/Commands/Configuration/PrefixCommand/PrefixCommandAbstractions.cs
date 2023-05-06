// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.PrefixCommand;
internal class PrefixCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.CurrentPrefix, ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.PrefixDisabled);

        return $"⌨ `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.PrefixDisabled).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].PrefixSettings.PrefixDisabled.ToEmote(ctx.Bot)}\n" +
               $"🗝 `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.PrefixConfigCommand.CurrentPrefix).PadRight(pad)}` : `{ctx.Bot.guilds[ctx.Guild.Id].PrefixSettings.Prefix}`";
    }
}
