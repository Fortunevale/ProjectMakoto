// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class InviteNoteEvents
{
    internal InviteNoteEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        Task.Run(async () =>
        {
            _bot.guilds[e.Guild.Id].InviteNotes.Notes.Remove(e.Invite.Code);
        }).Add(_bot.watcher);
    }
}
