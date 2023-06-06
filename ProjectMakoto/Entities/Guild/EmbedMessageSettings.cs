// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class EmbedMessageSettings
{
    public EmbedMessageSettings(Guild guild)
    {
        this.Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _UseEmbedding { get; set; } = false;
    public bool UseEmbedding
    {
        get => this._UseEmbedding;
        set
        {
            this._UseEmbedding = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "embed_messages", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _UseGithubEmbedding { get; set; } = false;
    public bool UseGithubEmbedding
    {
        get => this._UseGithubEmbedding;
        set
        {
            this._UseGithubEmbedding = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", this.Parent.ServerId, "embed_github", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
