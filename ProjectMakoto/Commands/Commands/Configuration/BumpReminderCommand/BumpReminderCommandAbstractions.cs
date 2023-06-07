// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.BumpReminderCommand;

internal sealed class BumpReminderCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.loadedTranslations.Commands.Config.BumpReminder;

        if (!ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled)
            return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `{CommandKey.BumpReminderEnabled.Get(ctx.DbUser)}` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}";

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.BumpReminderEnabled, CommandKey.BumpReminderChannel, CommandKey.BumpReminderRole);

        return $"{EmojiTemplates.GetQuestionMark(ctx.Bot)} `{CommandKey.BumpReminderEnabled.Get(ctx.DbUser).PadRight(pad)}` : {ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.Enabled.ToEmote(ctx.Bot)}\n" +
               $"{EmojiTemplates.GetChannel(ctx.Bot)} `{CommandKey.BumpReminderChannel.Get(ctx.DbUser).PadRight(pad)}` : <#{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.ChannelId})`\n" +
               $"{EmojiTemplates.GetUser(ctx.Bot)} `{CommandKey.BumpReminderRole.Get(ctx.DbUser).PadRight(pad)}` : <@&{ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId}> `({ctx.Bot.guilds[ctx.Guild.Id].BumpReminder.RoleId})`";
    }
}
