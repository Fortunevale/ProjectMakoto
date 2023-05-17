// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.LevelRewardsCommand;

internal class LevelRewardsCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = Bot.loadedTranslations.Commands.Config.LevelRewards;

        string str = "";
        if (ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Count != 0)
        {
            foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.OrderBy(x => x.Level))
            {
                if (!ctx.Guild.Roles.ContainsKey(b.RoleId))
                {
                    ctx.Bot.guilds[ctx.Guild.Id].LevelRewards.Remove(b);
                    continue;
                }

                str += $"**{ctx.BaseCommand.GetString(CommandKey.Level)}**: `{b.Level}`\n" +
                        $"**{ctx.BaseCommand.GetString(CommandKey.Role)}**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                        $"**{ctx.BaseCommand.GetString(CommandKey.Message)}**: `{b.Message}`\n";

                str += "\n\n";
            }
        }
        else
        {
            str = ctx.BaseCommand.GetString(CommandKey.NoRewardsSetup, true);
        }

        return str;
    }
}