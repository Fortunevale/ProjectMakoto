// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.AutoCrosspostCommand;

internal class AutoCrosspostCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"🤖 `Exclude Bots             `: {ctx.Bot.guilds[ctx.Guild.Id].Crosspost.ExcludeBots.ToEmote(ctx.Bot)}\n" +
               $"🕒 `Delay before crossposting`: `{TimeSpan.FromSeconds(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.DelayBeforePosting).GetHumanReadable()}`\n\n" +
               $"{(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Count != 0 ? string.Join("\n\n", ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : "`No Auto Crosspost Channels set up.`")}";
    }
}
