﻿namespace Project_Ichigo.Events;

internal class CommandEvents
{
    internal async Task CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            LogInfo($"Successfully executed '{e.Context.Prefix}{e.Command.Name}{(e.Context.RawArgumentString == "" ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ({e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds}ms)");

            try
            {
                await Task.Delay(2000);
                await e.Context.Message.DeleteAsync();
            }
            catch { }
        });
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        if (e.Command is not null)
            if (e.Exception.GetType().FullName == "System.ArgumentException")
            {
                _ = Task.Run(async () =>
                {
                    if (e.Command is not null)
                        LogWarn($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(e.Context.RawArgumentString == "" ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id}): {e.Exception}");

                    try
                    {
                        await e.Context.SendSyntaxError();
                    }
                    catch { }

                    try
                    {
                        await Task.Delay(2000);
                        await e.Context.Message.DeleteAsync();
                    }
                    catch { }
                });
            }
            else
            {
                _ = Task.Run(async () =>
                {
                    LogError($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(e.Context.RawArgumentString == "" ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name}  on '{e.Context.Guild.Name}' ({e.Context.Guild.Id}): {e.Exception}");

                    try
                    {
                        await e.Context.Channel.SendMessageAsync($"{e.Context.User.Mention}\n:warning: `I'm sorry but an unhandled exception occured while trying to execute your command.`\n\n" +
                                                                    $"```csharp\n" +
                                                                    $"{e.Exception}" +
                                                                    $"\n```");
                    }
                    catch { }

                    try
                    {
                        await Task.Delay(2000);
                        await e.Context.Message.DeleteAsync();
                    }
                    catch { }
                });
            }
    }
}
