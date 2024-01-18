// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

[TableName("globalnotes")]
internal class GlobalNote : RequiresBotReference
{
    public GlobalNote(Bot bot, ulong Id) : base(bot)
    {
        this.Id = Id;

        _ = this.Bot.DatabaseClient.CreateRow("globalnotes", typeof(GlobalNote), Id, this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("id"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }
    
    
    [ColumnName("notes"), ColumnType(ColumnTypes.LongText), Default("[]")]
    internal Note[] Notes
    {
        get => JsonConvert.DeserializeObject<Note[]>(this.Bot.DatabaseClient.GetValue<string>("globalnotes", "id", this.Id, "notes", this.Bot.DatabaseClient.mainDatabaseConnection));
        set => _ = this.Bot.DatabaseClient.SetValue("globalnotes", "id", this.Id, "notes", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    internal class Note
    {
        public string UUID { get; set; } = Guid.NewGuid().ToString();
        public string Reason { get; set; }
        public ulong Moderator { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
