// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.DevTools;

internal sealed class LogCommand : BaseCommand
{
    public override Task<bool> BeforeExecution(SharedCommandContext ctx) => this.CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var Level = (int)arguments["Level"];

            if (Level is > ((int)LogEventLevel.Fatal) or < ((int)LogEventLevel.Verbose))
                throw new Exception("Invalid Log Level");

            ctx.Bot.loggingLevel.MinimumLevel = (LogEventLevel)Level;
            _ = await this.RespondOrEdit($"`Changed LogLevel to '{Enum.GetName((LogEventLevel)Level)}'`");
        });
    }
}
