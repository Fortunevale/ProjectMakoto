﻿namespace ProjectMakoto.Events;

internal class InviteNoteEvents
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
