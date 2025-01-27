// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class InviteNotesDetails
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public Guild Parent { get; set; }

    private string _Invite { get; set; }
    public string Invite
    {
        get => this._Invite;
        set
        {
            this._Invite = value;
            this.Update();
        }
    }

    private string _Note { get; set; }
    public string Note
    {
        get => this._Note;
        set
        {
            this._Note = value;
            this.Update();
        }
    }

    private ulong _Moderator { get; set; }
    public ulong Moderator
    {
        get => this._Moderator;
        set
        {
            this._Moderator = value;
            this.Update();
        }
    }

    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.InviteNotes.Notes = this.Parent.InviteNotes.Notes.Update(x => x.Invite, this);
    }
}
