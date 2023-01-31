namespace ProjectMakoto.Util;

internal class TaskWatcher
{
    private List<TaskInfo> tasks = new();

    internal async void Watcher()
    {
        while (true)
        {
            foreach (var b in tasks.ToList())
            {
                if (b is null)
                    continue;

                if (!b.task.IsCompleted)
                    continue;

                var CommandContext = b.CommandContext;
                var InteractionContext = b.InteractionContext;
                var SharedCommandContext = b.SharedCommandContext;
                var ContextMenuContext = b.ContextMenuContext;

                if (b.task.IsCompletedSuccessfully)
                {
                    _logger.LogTrace("Successfully executed task:{Id} '{Uuid}' in {Elapsed}ms", b.task.Id, b.uuid, b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")));

                    if (SharedCommandContext is not null)
                        _logger.LogInfo("Successfully executed '{Prefix}{Name}' for '{User}' on '{Guild}'",
                            SharedCommandContext?.Prefix,
                            SharedCommandContext?.CommandName,
                            SharedCommandContext?.User?.Id,
                            SharedCommandContext?.Guild?.Id);
                    else if (CommandContext is not null)
                        _logger.LogInfo("Successfully executed '{Prefix}{Name}' for '{User}' on '{Guild}'",
                            CommandContext?.Prefix,
                            CommandContext?.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "" + CommandContext.Command.Name,
                            CommandContext?.User?.Id,
                            CommandContext?.Guild?.Id);
                    else if (InteractionContext is not null)
                        _logger.LogInfo("Successfully executed '/{Name}' for '{User}' on '{Guild}'",
                            InteractionContext?.CommandName,
                            InteractionContext?.User?.Id,
                            InteractionContext?.Guild?.Id);
                    else if (ContextMenuContext is not null)
                        _logger.LogInfo("Successfully executed '{Name}' for '{User}' on '{Guild}'",
                            ContextMenuContext?.CommandName,
                            ContextMenuContext?.User?.Id,
                            ContextMenuContext?.Guild?.Id);

                    tasks.Remove(b);
                    continue;
                }

                if (SharedCommandContext != null)
                    _logger.LogError("Failed to execute '{Prefix}{Name}' for '{User}' on '{Guild}'", b.task.Exception,
                        SharedCommandContext?.Prefix,
                        SharedCommandContext?.CommandName,
                        SharedCommandContext?.User?.Id,
                        SharedCommandContext?.Guild?.Id);
                else if (CommandContext != null)
                    _logger.LogError("Failed to executed '{Prefix}{Name}' for '{User}' on '{Guild}'", b.task.Exception,
                            CommandContext?.Prefix,
                            CommandContext?.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "" + CommandContext.Command.Name,
                            CommandContext?.User?.Id,
                            CommandContext?.Guild?.Id);
                else if (InteractionContext != null)
                    _logger.LogError("Failed to execute '/{Name}' for '{User}' on '{Guild}'", b.task.Exception,
                            InteractionContext?.CommandName,
                            InteractionContext?.User?.Id,
                            InteractionContext?.Guild?.Id);
                else if (ContextMenuContext != null)
                    _logger.LogError("Failed to execute '{Name}' for '{User}' on '{Guild}'", b.task.Exception,
                            ContextMenuContext?.CommandName,
                            ContextMenuContext?.User?.Id,
                            ContextMenuContext?.Guild?.Id);
                else
                    _logger.LogError("A task failed to execute", b.task.Exception);

                var ExceptionType = (b.task.Exception.GetType() != typeof(AggregateException) ? b.task.Exception.GetType() : b.task.Exception.InnerException.GetType());
                var Exception = (b.task.Exception.GetType() != typeof(AggregateException) ? b.task.Exception : b.task.Exception.InnerException);
                string ExceptionMessage = (b.task.Exception.GetType() != typeof(AggregateException) ? b.task.Exception.Message : b.task.Exception.InnerException.Message);

                if (ExceptionType == typeof(DisCatSharp.Exceptions.BadRequestException))
                {
                    try { _logger.LogError("WebRequestUrl: {Url}", ((DisCatSharp.Exceptions.BadRequestException)Exception).WebRequest.Url); } catch { }
                    try { _logger.LogError("WebRequest: {Request}", JsonConvert.SerializeObject(((DisCatSharp.Exceptions.BadRequestException)Exception).WebRequest, Formatting.Indented).Replace("\\", "")); } catch { }
                    try { _logger.LogError("WebResponse: {Response}", ((DisCatSharp.Exceptions.BadRequestException)Exception).WebResponse.Response); } catch { }
                }

                if (SharedCommandContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = SharedCommandContext.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
                        .WithContent($"{SharedCommandContext.User.Mention}\n⚠ `An unhandled exception occurred while trying to execute your command: '{ExceptionMessage.SanitizeForCode()}'`\n" +
                        $"`The exception has been automatically reported.`\n\n" +
                        $"\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                SharedCommandContext.BaseCommand.DeleteOrInvalidate();
                            });
                        });
                    }
                    catch (Exception ex) { _logger.LogError("Failed to notify user about unhandled exception.", ex); }

                // Backup handling in case the exception isn't caused via a command

                if (CommandContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = CommandContext.Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent($"{CommandContext.User.Mention}\n⚠ `An unhandled exception occurred while trying to execute your command: '{ExceptionMessage.SanitizeForCode()}'`\n" +
                        $"`The exception has been automatically reported.`\n\n" +
                        $"\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                _ = x.Result.DeleteAsync();
                            });
                        });
                    }
                    catch (Exception ex) { _logger.LogError("Failed to notify user about unhandled exception.", ex); }

                if (InteractionContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = InteractionContext.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{InteractionContext.User.Mention}\n⚠ `An unhandled exception occurred while trying to execute your command: '{ExceptionMessage.SanitizeForCode()}'`\n" +
                        $"`The exception has been automatically reported.`\n\n" +
                        $"\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                _ = InteractionContext.DeleteResponseAsync();
                            });
                        });
                    }
                    catch (Exception ex) { _logger.LogError("Failed to notify user about unhandled exception.", ex); }
                
                if (ContextMenuContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = ContextMenuContext.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{ContextMenuContext.User.Mention}\n⚠ `An unhandled exception occurred while trying to execute your command: '{ExceptionMessage.SanitizeForCode()}'`\n" +
                        $"`The exception has been automatically reported.`\n\n" +
                        $"\n\n_This message will be deleted {Formatter.Timestamp(DateTime.UtcNow.AddSeconds(11))}._")).ContinueWith(x =>
                        {
                            if (!x.IsCompletedSuccessfully)
                                return;

                            _ = Task.Delay(10000).ContinueWith(_ =>
                            {
                                _ = ContextMenuContext.DeleteResponseAsync();
                            });
                        });
                    }
                    catch (Exception ex) { _logger.LogError("Failed to notify user about unhandled exception.", ex); }

                tasks.Remove(b);
            }

            await Task.Delay(500);
        }
    }

    internal async void AddToList(TaskInfo taskInfo) => tasks.Add(taskInfo);

}
