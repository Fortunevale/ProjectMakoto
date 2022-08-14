﻿namespace ProjectIchigo.Util;

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
                    _logger.LogTrace($"Successfully executed task:{b.task.Id} '{b.uuid}' in {b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms");

                    if (CommandContext is not null)
                        _logger.LogInfo($"Successfully executed '{CommandContext.Prefix}{(CommandContext.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "")}{CommandContext.Command.Name}{(string.IsNullOrWhiteSpace(CommandContext.RawArgumentString) ? "" : $" {CommandContext.RawArgumentString}")}' for {CommandContext.User.UsernameWithDiscriminator} ({CommandContext.User.Id}) in #{CommandContext.Channel.Name} on '{CommandContext.Guild?.Name}' ({CommandContext.Guild?.Id}) ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)");
                    else if (InteractionContext is not null)
                        _logger.LogInfo($"Successfully executed '/{InteractionContext.CommandName}' for {InteractionContext.User.Username}#{InteractionContext.User.Discriminator} ({InteractionContext.User.Id}){(InteractionContext.Channel is not null ? $"in #{InteractionContext.Channel.Name}" : "")}{(InteractionContext.Guild is not null ? $" on '{InteractionContext.Guild.Name}' ({InteractionContext.Guild.Id})" : "")} ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)", b.task.Exception);
                    else if (SharedCommandContext is not null)
                        _logger.LogInfo($"Successfully executed '{SharedCommandContext.Prefix}{SharedCommandContext.CommandName}' for {SharedCommandContext.User.Username}#{SharedCommandContext.User.Discriminator} ({SharedCommandContext.User.Id}){(SharedCommandContext.Channel is not null ? $"in #{SharedCommandContext.Channel.Name}" : "")}{(SharedCommandContext.Guild is not null ? $" on '{SharedCommandContext.Guild.Name}' ({SharedCommandContext.Guild.Id})" : "")} ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)", b.task.Exception);
                    else if (ContextMenuContext is not null)
                        _logger.LogInfo($"Successfully executed '{ContextMenuContext.CommandName}' for {ContextMenuContext.User.Username}#{ContextMenuContext.User.Discriminator} ({ContextMenuContext.User.Id}){(ContextMenuContext.Channel is not null ? $"in #{ContextMenuContext.Channel.Name}" : "")}{(ContextMenuContext.Guild is not null ? $" on '{ContextMenuContext.Guild.Name}' ({ContextMenuContext.Guild.Id})" : "")} ({b.CreationTimestamp.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"))}ms)", b.task.Exception);

                    tasks.Remove(b);
                    continue;
                }

                if (CommandContext != null)
                    _logger.LogError($"Failed to execute '{CommandContext.Prefix}{(CommandContext.Command.Parent is not null ? $"{CommandContext.Command.Parent.Name} " : "")}{CommandContext.Command.Name}{(string.IsNullOrWhiteSpace(CommandContext.RawArgumentString) ? "" : $" {CommandContext.RawArgumentString}")}' for {CommandContext.User.Username}#{CommandContext.User.Discriminator} ({CommandContext.User.Id}) in #{CommandContext.Channel.Name} on '{CommandContext.Guild.Name}' ({CommandContext.Guild.Id})", b.task.Exception);
                else if (InteractionContext != null)
                    _logger.LogError($"Failed to execute '/{InteractionContext.CommandName}' for {InteractionContext.User.Username}#{InteractionContext.User.Discriminator} ({InteractionContext.User.Id}){(InteractionContext.Channel is not null ? $"in #{InteractionContext.Channel.Name}" : "")}{(InteractionContext.Guild is not null ? $" on '{InteractionContext.Guild.Name}' ({InteractionContext.Guild.Id})" : "")}", b.task.Exception);
                else if (ContextMenuContext != null)
                    _logger.LogError($"Failed to execute '{ContextMenuContext.CommandName}' for {ContextMenuContext.User.UsernameWithDiscriminator} ({ContextMenuContext.User.Id}){(ContextMenuContext.Channel is not null ? $"in #{ContextMenuContext.Channel.Name}" : "")}{(ContextMenuContext.Guild is not null ? $" on '{ContextMenuContext.Guild.Name}' ({ContextMenuContext.Guild.Id})" : "")}", b.task.Exception);
                else if (SharedCommandContext != null)
                    _logger.LogError($"Failed to execute '{SharedCommandContext.Prefix}{SharedCommandContext.CommandName}' for {SharedCommandContext.User.UsernameWithDiscriminator} ({SharedCommandContext.User.Id}){(SharedCommandContext.Channel is not null ? $"in #{SharedCommandContext.Channel.Name}" : "")}{(SharedCommandContext.Guild is not null ? $" on '{SharedCommandContext.Guild.Name}' ({SharedCommandContext.Guild.Id})" : "")}", b.task.Exception);
                else
                    _logger.LogError($"A non-command task failed to execute", b.task.Exception);

                if (b.task.Exception.InnerException.GetType() == typeof(DisCatSharp.Exceptions.BadRequestException))
                {
                    try { _logger.LogError($"WebRequestUrl: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest.Url}"); } catch { }
                    try { _logger.LogError($"WebRequest: {JsonConvert.SerializeObject(((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest, Formatting.Indented).Replace("\\", "")}"); } catch { }
                    try { _logger.LogError($"WebResponse: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebResponse.Response}"); } catch { }
                }

                var ExceptionType = (b.task.Exception.GetType() != typeof(AggregateException) ? b.task.Exception.GetType() : b.task.Exception.InnerException.GetType());
                string ExceptionMessage = (b.task.Exception.GetType() != typeof(AggregateException) ? b.task.Exception.Message : b.task.Exception.InnerException.Message);

                if (CommandContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = CommandContext.Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent($"{CommandContext.User.Mention}\n⚠ `An unhandled exception occured while trying to execute your request: '{ExceptionMessage.SanitizeForCodeBlock()}'`\n\n" +
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
                        .WithContent($"{InteractionContext.User.Mention}\n⚠ `An unhandled exception occured while trying to execute your request: '{ExceptionMessage.SanitizeForCodeBlock()}'`\n\n" +
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
                
                if (SharedCommandContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = SharedCommandContext.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
                        .WithContent($"{SharedCommandContext.User.Mention}\n⚠ `An unhandled exception occured while trying to execute your request: '{ExceptionMessage.SanitizeForCodeBlock()}'`\n\n" +
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
                
                if (ContextMenuContext != null && ExceptionType != typeof(DisCatSharp.Exceptions.NotFoundException))
                    try
                    {
                        _ = ContextMenuContext.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"{ContextMenuContext.User.Mention}\n⚠ `An unhandled exception occured while trying to execute your request: '{ExceptionMessage.SanitizeForCodeBlock()}'`\n\n" +
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

                tasks.Remove(b);
            }

            await Task.Delay(500);
        }
    }

    internal async void AddToList(TaskInfo taskInfo) => tasks.Add(taskInfo);

}
