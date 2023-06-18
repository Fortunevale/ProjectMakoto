// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
internal sealed class MessageComponents
{
    public static DiscordButtonComponent GetCancelButton(User user, Bot _bot)
        => new(ButtonStyle.Secondary, "cancel", _bot.LoadedTranslations.Common.Cancel.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));

    public static DiscordButtonComponent GetBackButton(User user, Bot _bot)
        => new(ButtonStyle.Secondary, "back", _bot.LoadedTranslations.Common.Back.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
}
