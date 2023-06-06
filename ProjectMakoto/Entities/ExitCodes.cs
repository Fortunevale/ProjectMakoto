// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal enum ExitCodes
{
    VitalTaskFailed = 1,
    ExitTasksTimeout = 21,

    NoToken = 8,
    FailedDiscordLogin = 9,

    FailedDatabaseLoad = 18,
    FailedDatabaseLogin = 19,
}