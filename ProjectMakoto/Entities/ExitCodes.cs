// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal class ExitCodes
{
    internal static readonly int ExitTasksTimeout = 21;

    internal static readonly int NoToken = 8;
    internal static readonly int FailedDiscordLogin = 9;

    internal static readonly int FailedDatabaseLoad = 18;
    internal static readonly int FailedDatabaseLogin = 19;
}
