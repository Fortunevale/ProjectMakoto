﻿// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;
public sealed class ScheduledTaskIdentifier
{
    public ScheduledTaskIdentifier(ulong Snowflake, string Id, string Type)
    {
        this.Snowflake = Snowflake;
        this.Id = Id;
        this.Type = Type;
    }

    public ulong Snowflake { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
}
