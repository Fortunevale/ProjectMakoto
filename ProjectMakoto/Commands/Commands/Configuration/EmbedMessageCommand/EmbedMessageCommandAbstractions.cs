// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.EmbedMessageCommand;

internal class EmbedMessageCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        var CommandKey = ctx.Bot.loadedTranslations.Commands.Config.EmbedMessages;

        var pad = TranslationUtil.CalculatePadding(ctx.DbUser, CommandKey.EmbedGithubCode, CommandKey.EmbedMessageLinks);

        return $"{"💬".UnicodeToEmoji()} `{CommandKey.EmbedMessageLinks.Get(ctx.DbUser).PadRight(pad)}`: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseEmbedding.ToEmote(ctx.Bot)}\n" +
               $"{"🤖".UnicodeToEmoji()} `{CommandKey.EmbedGithubCode.Get(ctx.DbUser).PadRight(pad)}`: {ctx.Bot.guilds[ctx.Guild.Id].EmbedMessage.UseGithubEmbedding.ToEmote(ctx.Bot)}";
    }
}
