﻿namespace ProjectIchigo.Commands;

internal class LogCommand : BaseCommand
{
    public override async Task<bool> BeforeExecution(SharedCommandContext ctx) => await CheckMaintenance();

    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            LogLevel Level = (LogLevel)arguments["Level"];

            if (Level > Xorog.Logger.Enums.LogLevel.TRACE2)
                throw new Exception("Invalid Log Level");

            _logger.ChangeLogLevel(Level);
            await RespondOrEdit($"`Changed LogLevel to '{(LogLevel)Level}'`");
        });
    }
}
