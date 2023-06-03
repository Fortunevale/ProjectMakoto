// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMakoto.PrefixCommands;
public class PrefixCommandUtil
{
    public static async Task SendGroupHelp(Bot _bot, CommandContext ctx, string CommandName, string CustomText = "", string CustomImageUrl = "", string CustomParentName = "")
    {
        if (await _bot.users[ctx.User.Id].Cooldown.WaitForLight(new SharedCommandContext(ctx.Message, _bot, CommandName)))
            return;

        if (ctx.Command.Parent is not null)
            await ctx.Command.Parent.Children.SendCommandGroupHelp(ctx, CustomText, CustomImageUrl, CustomParentName);
        else
            await ((CommandGroup)ctx.Command).Children.SendCommandGroupHelp(ctx, CustomText, CustomImageUrl, CustomParentName);
    }
}
