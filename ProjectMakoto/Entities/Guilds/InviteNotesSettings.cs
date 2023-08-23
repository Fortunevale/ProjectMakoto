// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;

namespace ProjectMakoto.Entities.Guilds;

public sealed class InviteNotesSettings : RequiresParent<Guild>
{
    public InviteNotesSettings(Bot bot, Guild parent) : base(bot, parent)
    {
    }

    [ColumnName("invitenotes"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public InviteNotesDetails[] Notes
    {
        get => JsonConvert.DeserializeObject<InviteNotesDetails[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "invitenotes", this.Bot.DatabaseClient.mainDatabaseConnection))
            .Select(x =>
            {
                x.Bot = this.Bot;
                x.Parent = this.Parent;

                return x;
            }).ToArray();
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "invitenotes", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
