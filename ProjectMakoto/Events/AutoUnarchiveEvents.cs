// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class AutoUnarchiveEvents
{
    internal AutoUnarchiveEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task ThreadUpdated(DiscordClient sender, ThreadUpdateEventArgs e)
    {
        await Task.Delay(5000);
        if (this._bot.guilds[e.Guild.Id].AutoUnarchiveThreads.Contains(e.ThreadAfter.Parent.Id))
        {
            if (e.ThreadAfter.ThreadMetadata.Archived && (!e.ThreadAfter.ThreadMetadata.Locked ?? false))
                _ = e.ThreadAfter.UnarchiveAsync();
        }
    }
}
