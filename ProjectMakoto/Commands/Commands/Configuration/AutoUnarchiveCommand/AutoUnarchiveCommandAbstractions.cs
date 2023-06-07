// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.AutoUnarchiveCommand;

internal sealed class AutoUnarchiveCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        foreach (var b in ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.ToList())
        {
            if (!ctx.Guild.Channels.ContainsKey(b))
                ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Remove(b);
        }

        return $"{(ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Any() ? string.Join("\n", ctx.Bot.guilds[ctx.Guild.Id].AutoUnarchiveThreads.Select(x => $"{ctx.Guild.GetChannel(x).Mention} [`#{ctx.Guild.GetChannel(x).Name}`] (`{x}`)")) : ctx.Bot.loadedTranslations.Commands.Config.AutoUnarchive.NoChannels.Get(ctx.DbUser).Build(true))}";
    }
}
