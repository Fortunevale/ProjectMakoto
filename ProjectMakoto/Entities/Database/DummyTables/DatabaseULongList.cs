﻿// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Database;

[TableName("/")]
public class DatabaseULongList
{
    [ColumnName("id"), ColumnType(ColumnTypes.BigInt), Primary]
    public ulong Id { get; init; }
}
