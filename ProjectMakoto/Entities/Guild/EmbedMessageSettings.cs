// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public class EmbedMessageSettings
{
    public EmbedMessageSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }



    private bool _UseEmbedding { get; set; } = false;
    public bool UseEmbedding
    {
        get => _UseEmbedding;
        set
        {
            _UseEmbedding = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "embed_messages", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }

    private bool _UseGithubEmbedding { get; set; } = false;
    public bool UseGithubEmbedding
    {
        get => _UseGithubEmbedding;
        set
        {
            _UseGithubEmbedding = value;
            _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "embed_github", value, Bot.DatabaseClient.mainDatabaseConnection);
        }
    }
}
