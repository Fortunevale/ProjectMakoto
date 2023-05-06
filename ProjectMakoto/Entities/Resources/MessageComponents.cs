// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
internal class MessageComponents
{
    public static DiscordButtonComponent GetCancelButton(User user)
        => new(ButtonStyle.Secondary, "cancel", Bot.loadedTranslations.Common.Cancel.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("❌")));
    
    public static DiscordButtonComponent GetBackButton(User user)
        => new(ButtonStyle.Secondary, "back", Bot.loadedTranslations.Common.Back.Get(user), false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
}
