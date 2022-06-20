namespace ProjectIchigo;

internal class TaskWatcher
{
    private List<TaskInfo> tasks = new();

    internal async void Watcher()
    {
        while (true)
        {
            foreach (var b in tasks.ToList())
            {
                if (!b.task.IsCompleted)
                    continue;

                var CommandContext = b.CommandContext;
                var InteractionContext = b.InteractionContext;

                if (b.task.IsCompletedSuccessfully)
                {
                    _logger.LogTrace($"Successfully executed task:{b.task.Id} '{b.uuid}' in {b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms");

                    if (CommandContext is not null)
                        _logger.LogInfo($"Successfully executed '{CommandContext.Prefix}{(CommandContext.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "")}{CommandContext.Command.Name}{(string.IsNullOrWhiteSpace(CommandContext.RawArgumentString) ? "" : $" {CommandContext.RawArgumentString}")}' for {CommandContext.User.Username}#{CommandContext.User.Discriminator} ({CommandContext.User.Id}) in #{CommandContext.Channel.Name} on '{CommandContext.Guild.Name}' ({CommandContext.Guild.Id}) ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)");
                    else if (InteractionContext is not null)
                        _logger.LogInfo($"Successfully executed '/{InteractionContext.CommandName}' for {InteractionContext.User.Username}#{InteractionContext.User.Discriminator} ({InteractionContext.User.Id}){(InteractionContext.Channel is not null ? $"in #{InteractionContext.Channel.Name}" : "")}{(InteractionContext.Guild is not null ? $" on '{InteractionContext.Guild.Name}' ({InteractionContext.Guild.Id})" : "")} ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)", b.task.Exception);

                    tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
                    continue;
                }

                if (CommandContext != null)
                    _logger.LogError($"Failed to execute '{CommandContext.Prefix}{(CommandContext.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "")}{CommandContext.Command.Name}{(string.IsNullOrWhiteSpace(CommandContext.RawArgumentString) ? "" : $" {CommandContext.RawArgumentString}")}' for {CommandContext.User.Username}#{CommandContext.User.Discriminator} ({CommandContext.User.Id}) in #{CommandContext.Channel.Name} on '{CommandContext.Guild.Name}' ({CommandContext.Guild.Id})", b.task.Exception);
                else if (InteractionContext != null)
                    _logger.LogError($"Failed to execute '/{InteractionContext.CommandName}' for {InteractionContext.User.Username}#{InteractionContext.User.Discriminator} ({InteractionContext.User.Id}){(InteractionContext.Channel is not null ? $"in #{InteractionContext.Channel.Name}" : "")}{(InteractionContext.Guild is not null ? $" on '{InteractionContext.Guild.Name}' ({InteractionContext.Guild.Id})" : "")}", b.task.Exception);
                else
                    _logger.LogError($"A non-command task failed to execute: {b.task.Exception}");

                if (b.task.Exception.InnerException.GetType() == typeof(DisCatSharp.Exceptions.BadRequestException))
                {
                    _logger.LogError($"WebRequestUrl: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest.Url}\n\n" +
                            $"WebRequest: {JsonConvert.SerializeObject(((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest, Formatting.Indented).Replace("\\", "")}\n\n" +
                            $"WebResponse: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebResponse.Response}");
                }

                if (CommandContext != null)
                    try
                    {
                        _ = CommandContext.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{CommandContext.User.Mention}\n:warning: `An unhandled exception occured while trying to execute your request.`\n\n" +
                        $"```csharp\n" +
                        $"{b.task.Exception}" +
                        $"\n```\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                _ = x.Result.DeleteAsync();
                            });
                        });
                    }
                    catch { }

                if (InteractionContext != null)
                    try
                    {
                        _ = InteractionContext.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{InteractionContext.User.Mention}\n:warning: `An unhandled exception occured while trying to execute your request.`\n\n" +
                        $"```csharp\n" +
                        $"{b.task.Exception}" +
                        $"\n```\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                _ = x.Result.DeleteAsync();
                            });
                        });
                    }
                    catch (Exception ex) { _logger.LogError("a", ex); }

                tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
            }

            await Task.Delay(1000);
        }
    }

    internal async void AddToList(TaskInfo taskInfo) => tasks.Add(taskInfo);

}
