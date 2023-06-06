// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal class LogCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            CustomLogLevel Level = (CustomLogLevel)arguments["Level"];

            if (Level > CustomLogLevel.Trace2)
                throw new Exception("Invalid Log Level");

            _logger.ChangeLogLevel(Level);
            await RespondOrEdit($"`Changed LogLevel to '{(CustomLogLevel)Level}'`");
        });
    }
}
