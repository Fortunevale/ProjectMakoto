// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class InviteTrackerSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("invitetracker_enabled"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool Enabled
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "invitetracker_enabled", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "invitetracker_enabled", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("invitetracker_cache"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public InviteTrackerCacheItem[] Cache
    {
        get => JsonConvert.DeserializeObject<InviteTrackerCacheItem[]>(this.Bot.DatabaseClient.GetValue<string>("guilds", "serverid", this.Parent.Id, "invitetracker_cache", this.Bot.DatabaseClient.mainDatabaseConnection))
            .Select(x =>
            {
                x.Bot = this.Bot;
                x.Parent = this.Parent;

                return x;
            }).ToArray();
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "invitetracker_cache", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
