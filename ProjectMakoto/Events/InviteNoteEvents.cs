// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Events;

internal sealed class InviteNoteEvents(Bot bot) : RequiresTranslation(bot)
{
    internal async Task InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
    {
        if (this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes.Any(x => x.Invite == e.Invite.Code))
            this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes = this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes
                .Remove(x => x.Invite, this.Bot.Guilds[e.Guild.Id].InviteNotes.Notes.First(x => x.Invite == e.Invite.Code));
    }
}
