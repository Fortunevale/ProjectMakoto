// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;

internal sealed class CommandEvents
{
    internal CommandEvents(Bot _bot)
    {
        this._bot = _bot;
    }

    public Bot _bot { private get; set; }

    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        Task.Run(async () =>
        {
            _logger.LogDebug("Successfully started execution of '{Prefix}{Name}' for {User} on {Guild} ({ResponseTime}ms)", 
            e.Context.Prefix, 
            (e.Command.Parent is not null ? $"{e.Command.Parent.Name} " : "") + e.Command.Name, 
            e.Context.User.Id,
            e.Context.Guild?.Id,
            e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds);

            try
            {
                if (e.Command.CustomAttributes.Any(x => x.GetType() == typeof(PreventCommandDeletionAttribute)))
                {
                    if (e.Command.CustomAttributes.OfType<PreventCommandDeletionAttribute>().FirstOrDefault().PreventDeleteCommandMessage)
                        return;
                }
            }
            catch { }

            _ = Task.Delay(2000).ContinueWith(x =>
            {
                _ = e.Context.Message.DeleteAsync();
            });
        }).Add(_bot.watcher);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        if (e.Command is not null)
            if (e.Exception.GetType() == typeof(ArgumentException))
            {
                Task.Run(async () =>
                {
                    if (e.Command is not null)
                        _logger.LogWarn("Failed to execute '{Prefix}{Name}' for {User} on {Guild} ({ResponseTime}ms)",
                            e.Context.Prefix,
                            (e.Command.Parent is not null ? $"{e.Command.Parent.Name} " : "") + e.Command.Name,
                            e.Context.User.Id,
                            e.Context.Guild?.Id,
                            e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds);

                    _ = e.Context.SendSyntaxError();

                    _ = Task.Delay(2000).ContinueWith(x =>
                    {
                        _ = e.Context.Message.DeleteAsync();
                    });
                }).Add(_bot.watcher);
            }
            else if (e.Exception.GetType() == typeof(CancelException))
            {
                return;
            }
            else
            {
                Task.Run(async () =>
                {
                    _logger.LogError("Failed to execute '{Prefix}{Name}' for {User} on {Guild} ({ResponseTime}ms)",
                        e.Context.Prefix,
                        (e.Command.Parent is not null ? $"{e.Command.Parent.Name} " : "") + e.Command.Name,
                        e.Context.User.Id,
                        e.Context.Guild?.Id,
                        e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds);

                    try
                    {
                        _ = e.Context.Channel.SendMessageAsync($"{e.Context.User.Mention}\n:warning: `I'm sorry but an unhandled exception occurred while trying to execute your command.`");
                    }
                    catch { }

                    _ = Task.Delay(2000).ContinueWith(x =>
                    {
                        _ = e.Context.Message.DeleteAsync();
                    });
                }).Add(_bot.watcher);
            }
    }
}
