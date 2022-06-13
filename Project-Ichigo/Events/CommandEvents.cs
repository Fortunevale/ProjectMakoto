namespace ProjectIchigo.Events;

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
            LogDebug($"Successfully started execution of '{e.Context.Prefix}{(e.Command.Parent is not null ? $"{e.Command.Parent.Name} " : "")}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ({e.Context.Message.CreationTimestamp.GetTimespanSince().Milliseconds}ms)");

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
        }).Add(_bot._watcher);
    }

    internal async Task CommandError(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        if (e.Command is not null)
            if (e.Exception.GetType() == typeof(ArgumentException))
            {
                Task.Run(async () =>
                {
                    if (e.Command is not null)
                        LogWarn($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name} on '{e.Context.Guild.Name}' ({e.Context.Guild.Id})", e.Exception);

                    _ = e.Context.SendSyntaxError();

                    _ = Task.Delay(2000).ContinueWith(x =>
                    {
                        _ = e.Context.Message.DeleteAsync();
                    });
                }).Add(_bot._watcher);
            }
            else if (e.Exception.GetType() == typeof(CancelCommandException))
            {
                return;
            }
            else
            {
                Task.Run(async () =>
                {
                    LogError($"Failed to execute '{e.Context.Prefix}{e.Command.Name}{(string.IsNullOrWhiteSpace(e.Context.RawArgumentString) ? "" : e.Context.RawArgumentString.Insert(0, " "))}' for {e.Context.User.Username}#{e.Context.User.Discriminator} ({e.Context.User.Id}) in #{e.Context.Channel.Name}  on '{e.Context.Guild.Name}' ({e.Context.Guild.Id})", e.Exception);

                    try
                    {
                        _ = e.Context.Channel.SendMessageAsync($"{e.Context.User.Mention}\n:warning: `I'm sorry but an unhandled exception occured while trying to execute your command.`");
                    }
                    catch { }

                    _ = Task.Delay(2000).ContinueWith(x =>
                    {
                        _ = e.Context.Message.DeleteAsync();
                    });
                }).Add(_bot._watcher);
            }
    }
}
