namespace Project_Ichigo.Events;

internal class CommandEvents
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
            LogDebug($"Successfully started execution of '{e.Context.Prefix}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ({e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds}ms)");

            try
            {
                await Task.Delay(2000);
                await e.Context.Message.DeleteAsync();
            }
            catch { }
        }).Add(_bot._watcher);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        if (e.Command is not null)
            if (e.Exception.GetType().FullName == "System.ArgumentException")
            {
                Task.Run(async () =>
                {
                    if (e.Command is not null)
                        LogWarn($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id})", e.Exception);

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
                }).Add(_bot._watcher);
            }
            else
            {
                Task.Run(async () =>
                {
                    LogError($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name}  on '{e.Context.Guild.Name}' ({e.Context.Guild.Id})", e.Exception);

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
                }).Add(_bot._watcher);
            }
    }
}
