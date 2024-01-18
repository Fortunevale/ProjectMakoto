// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class EmbedMessageSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("embed_messages"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool UseEmbedding
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "embed_messages",  this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "embed_messages", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("embed_github"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool UseGithubEmbedding
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "embed_github", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "embed_github", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
