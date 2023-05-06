// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.BumpReminderCommand;

internal class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled)
            return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}";

        return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `Bump Reminder Enabled` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Bot)} `Bump Reminder Channel` : <#{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId})`\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `Bump Reminder Role   ` : <@&{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId})`";
    }
}
