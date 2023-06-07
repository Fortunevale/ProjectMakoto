// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.AutoCrosspostCommand;

internal sealed class AutoCrosspostCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.loadedTranslations.Commands.Config.AutoCrosspost;

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.ExcludeBots, CommandKey.DelayBeforePosting);

        return $"🤖 `{CommandKey.ExcludeBots.Get(ctx.DbUser).PadRight(pad)}`: {ctx.Bot.guilds[ctx.Guild.Id].Crosspost.ExcludeBots.ToEmote(ctx.Bot)}\n" +
               $"🕒 `{CommandKey.DelayBeforePosting.Get(ctx.DbUser).PadRight(pad)}`: `{TimeSpan.FromSeconds(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.DelayBeforePosting).GetHumanReadable()}`\n\n" +
               $"{(ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Count != 0 ? string.Join("\n\n", ctx.Bot.guilds[ctx.Guild.Id].Crosspost.CrosspostChannels.Select(x => $"<#{x}> `[#{ctx.Guild.GetChannel(x).Name}]`")) : CommandKey.NoCrosspostChannels.Get(ctx.DbUser).Build(true))}";
    }
}
