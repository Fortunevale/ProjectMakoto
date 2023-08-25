// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto;

[TableName("scam_urls")]
public sealed class PhishingUrlEntry : RequiresBotReference
{
    public PhishingUrlEntry(Bot bot, string Url) : base(bot)
    {
        if (Url.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(Url));

        if (!this.Bot.PhishingHosts.ContainsKey(Url))
            if (!this.Bot.DatabaseClient.CreateRow("scam_urls", typeof(PhishingUrlEntry), Url, this.Bot.DatabaseClient.mainDatabaseConnection))
                throw new Exception("Failed to create new row");

        this.Url = Url;
    }

    [ColumnName("url"), ColumnType(ColumnTypes.VarChar), Collation("utf8mb4_0900_ai_ci"), MaxValue(500), Primary]
    public string Url { get; init; }

    [ColumnName("origin"), ColumnType(ColumnTypes.LongText), Collation("utf8mb4_0900_ai_ci"), Default("[]")]
    public string[] Origin
    {
        get => JsonConvert.DeserializeObject<string[]>(this.Bot.DatabaseClient.GetValue<string>("scam_urls", "url", this.Url, "origin", this.Bot.DatabaseClient.mainDatabaseConnection) ?? "");
        set => _ = this.Bot.DatabaseClient.SetValue("scam_urls", "url", this.Url, "origin", JsonConvert.SerializeObject(value), this.Bot.DatabaseClient.mainDatabaseConnection);
    }

    [ColumnName("submitter"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public ulong Submitter
    {
        get => this.Bot.DatabaseClient.GetValue<ulong>("scam_urls", "url", this.Url, "submitter", this.Bot.DatabaseClient.mainDatabaseConnection);
        set => _ = this.Bot.DatabaseClient.SetValue("scam_urls", "url", this.Url, "submitter", value, this.Bot.DatabaseClient.mainDatabaseConnection);
    }
}