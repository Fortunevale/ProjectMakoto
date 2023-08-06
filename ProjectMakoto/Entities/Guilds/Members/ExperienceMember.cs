// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Guilds;

namespace ProjectMakoto.Entities.Members;

public sealed class ExperienceMember : RequiresParent<Member>
{
    public ExperienceMember(Bot bot, Member parent) : base(bot, parent)
    {
    }

    private DateTime _Last_Message { get; set; } = DateTime.UnixEpoch;
    public DateTime Last_Message
    {
        get => this._Last_Message;
        set
        {
            this._Last_Message = value;
            _ = this.Bot.DatabaseClient.UpdateValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_last_message", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }

    private long _Points { get; set; } = 1;
    public long Points
    {
        get => this._Points;
        set
        {
            this._Points = value;
            _ = this.Bot.DatabaseClient.UpdateValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }

    private long _Level { get; set; } = 1;
    public long Level
    {
        get
        {
            return this._Level <= 0 ? 1 : this._Level;
        }
        set
        {
            this._Level = value;
            _ = this.Bot.DatabaseClient.UpdateValue(this.Parent.Parent.Id.ToString(), "userid", this.Parent.Id, "experience_level", value, this.Bot.DatabaseClient.guildDatabaseConnection);
        }
    }
}
