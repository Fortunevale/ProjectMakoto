// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class InviteTrackerMember : RequiresParent<Member>
{
    public InviteTrackerMember(Bot bot, Member parent) : base(bot, parent)
    {
    }

    private ulong _UserId { get; set; } = 0;
    public ulong UserId
    {
        get => this._UserId;
        set
        {
            this._UserId = value;
            _ = Bot.DatabaseClient.UpdateValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_user", value, Bot.DatabaseClient.guildDatabaseConnection);
        }
    }

    private string _Code { get; set; } = "";
    public string Code
    {
        get => this._Code;
        set
        {
            this._Code = value;
            _ = Bot.DatabaseClient.UpdateValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "invite_code", value, Bot.DatabaseClient.guildDatabaseConnection);
        }
    }
}
