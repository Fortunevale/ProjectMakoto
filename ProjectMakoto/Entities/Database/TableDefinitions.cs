// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Entities.Database.ColumnAttributes;
using ProjectMakoto.Entities.Database.ColumnTypes;

namespace ProjectMakoto.Entities.Database;

public sealed class TableDefinitions
{
    public sealed class scam_urls
    {
        [Primary]
        [Collation("utf8mb4_0900_ai_ci")]
        [MaxValue(500)]
        public VarChar url { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        public Text origin { get; set; }

        public BigInt submitter { get; set; }
    }

    public sealed class objected_users
    {
        [Primary]
        public BigInt id { get; set; }
    }

    public sealed class globalbans
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }

    public sealed class banned_users
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }

    public sealed class banned_guilds
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        public Text reason { get; set; }

        public BigInt moderator { get; set; }

        public BigInt timestamp { get; set; }
    }

    public sealed class globalnotes
    {
        [Primary]
        public BigInt id { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        public LongText notes { get; set; }
    }

    public sealed class active_url_submissions
    {
        [Primary]
        public BigInt messageid { get; set; }

        [Collation("utf8mb4_0900_ai_ci")]
        [MaxValue(500)]
        public VarChar url { get; set; }

        public BigInt submitter { get; set; }
        public BigInt guild { get; set; }
    }
}
