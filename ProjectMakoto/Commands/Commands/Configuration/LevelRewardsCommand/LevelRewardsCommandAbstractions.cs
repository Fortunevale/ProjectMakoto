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

                str += $"**Level**: `{b.Level}`\n" +
                        $"**Role**: <@&{b.RoleId}> (`{b.RoleId}`)\n" +
                        $"**Message**: `{b.Message}`\n";

                str += "\n\n";
            }
        }
        else
        {
            str = $"`No Level Rewards are set up.`";
        }

        return str;
    }
}