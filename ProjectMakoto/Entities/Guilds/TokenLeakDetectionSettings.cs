// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class TokenLeakDetectionSettings(Bot bot, Guild parent) : RequiresParent<Guild>(bot, parent)
{
    [ColumnName("tokens_detect"), ColumnType(ColumnTypes.TinyInt), Default("1")]
    public bool DetectTokens
    {
        get => this.Bot.DatabaseClient.GetValue<bool>("guilds", "serverid", this.Parent.Id, "tokens_detect", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("guilds", "serverid", this.Parent.Id, "tokens_detect", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}
