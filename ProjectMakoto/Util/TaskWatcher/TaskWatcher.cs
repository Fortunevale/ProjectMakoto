// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public sealed class TaskWatcher
{
    internal TaskWatcher()
    {
        this.Watcher();
    }

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
                            InteractionContext?.FullCommandName,
                            InteractionContext?.User?.Id,
                            InteractionContext?.Guild?.Id);
                    else if (ContextMenuContext is not null)
                        _logger.LogInfo("Successfully executed '{Name}' for '{User}' on '{Guild}'",
                            ContextMenuContext?.FullCommandName,
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
                            InteractionContext?.FullCommandName,
                            InteractionContext?.User?.Id,
                            InteractionContext?.Guild?.Id);
                else if (ContextMenuContext != null)
                    _logger.LogError("Failed to execute '{Name}' for '{User}' on '{Guild}'", b.task.Exception,
                            ContextMenuContext?.FullCommandName,
                            ContextMenuContext?.User?.Id,
                            ContextMenuContext?.Guild?.Id);
                else
                    _logger.LogError("A task failed to execute", b.task.Exception);

                if (b.IsVital)
                {
                    await Task.Delay(1000);
                    Environment.Exit((int)ExitCodes.VitalTaskFailed);
                    return;
                }

                var ExceptionType = (b.task.Exception?.GetType() != typeof(AggregateException) ? b.task.Exception?.GetType() : b.task.Exception?.InnerException.GetType());
                var Exception = (b.task.Exception?.GetType() != typeof(AggregateException) ? b.task.Exception : b.task.Exception.InnerException);
                string ExceptionMessage = (b.task.Exception?.GetType() != typeof(AggregateException) ? b.task.Exception?.Message : b.task.Exception.InnerException?.Message);

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

    internal TaskInfo AddToList(TaskInfo taskInfo)
    {
        tasks.Add(taskInfo);
        return taskInfo;
    }

    internal async static void LogHandler(Bot bot, object? sender, LogMessageEventArgs e)
    {
        switch (e.LogEntry.LogLevel)
        {
            case CustomLogLevel.Fatal:
            case CustomLogLevel.Error:
            {
                try
                {
                    if (bot.status.DiscordInitialized)
                    {
                        if (e.LogEntry.Message is "[111] Connection terminated (4000, ''), reconnecting"
                            or "[111] Connection terminated (-1, ''), reconnecting"
                            or "[111] Connection terminated (1001, 'CloudFlare WebSocket proxy restarting'), reconnecting")
                            break;

                        var channel = bot.discordClient.Guilds[bot.status.LoadedConfig.Channels.Assets].GetChannel(bot.status.LoadedConfig.Channels.ExceptionLog);

                        DiscordEmbedBuilder template = new DiscordEmbedBuilder()
                                                    .WithColor(e.LogEntry.LogLevel == CustomLogLevel.Fatal ? new DiscordColor("#FF0000") : EmbedColors.Error)
                                                    .WithTitle(e.LogEntry.LogLevel.GetName().ToLower().FirstLetterToUpper())
                                                    .WithTimestamp(e.LogEntry.TimeOfEvent);

                        List<DiscordEmbedBuilder> embeds = new();

                        if (e.LogEntry.Exception is not null)
                        {
                            void BuildEmbed(Exception ex, bool First)
                            {
                                var embed = new DiscordEmbedBuilder(template);

                                if (First)
                                    embed.WithDescription($"`{e.LogEntry.Message.SanitizeForCode()}`");
                                else
                                {
                                    embed.Title = "";
                                }

                                embed.AddField(new DiscordEmbedField("Message", $"```{ex.Message.SanitizeForCode()}```"));
                                if (!ex.StackTrace.IsNullOrWhiteSpace())
                                {
                                    string regex = @"((?:(?:(?:[A-Z]:\\)|(?:\/))[^\\\/]*[\\\/]).*):line (\d{0,10})";
                                    var b = Regex.Matches(ex.StackTrace, regex);

                                    if (b.Count > 0)
                                    {
                                        embed.AddField(new DiscordEmbedField("Stack Trace", $"```{Regex.Replace(ex.StackTrace, "in " + regex, "").Replace("   at ", "")}```"));
                                        embed.AddField(new DiscordEmbedField("File", $"```{b[0].Groups[1]}```"));
                                        embed.AddField(new DiscordEmbedField("Line", $"`{b[0].Groups[2]}`"));
                                    }
                                    else
                                    {
                                        embed.AddField(new DiscordEmbedField("Stack Trace", $"```{ex.StackTrace?.SanitizeForCode()}```"));
                                    }
                                }
                                else
                                {
                                    embed.AddField(new DiscordEmbedField("Stack Trace", $"```No Stack Trace captured.```"));
                                }

                                embed.AddField(new DiscordEmbedField("Source", $"`{ex.Source?.SanitizeForCode() ?? "No Source captured."}`", true));
                                embed.AddField(new DiscordEmbedField("Throwing Method", $"`{ex.TargetSite?.Name ?? "No Method captured"}` in `{ex.TargetSite?.DeclaringType?.Name ?? "No Type captured."}`", true));
                                embed.WithFooter(ex.HResult.ToString());

                                if ((ex.Data?.Keys?.Count ?? 0) > 0)
                                    embed.AddFields(ex.Data.Keys.Cast<object>().ToDictionary(k => k.ToString(), v => ex.Data[v]).Select(x => new DiscordEmbedField(x.Key, x.Value.ToString(), true)));

                                embeds.Add(embed);

                                if (ex is AggregateException aggr)
                                    foreach (var b in aggr.InnerExceptions)
                                    {
                                        BuildEmbed(b, false);
                                    }
                                else if (ex.InnerException is not null)
                                    BuildEmbed(ex.InnerException, false);
                            }

                            BuildEmbed(e.LogEntry.Exception, true);
                        }

                        int index = 0;

                        while (index < embeds.Count)
                        {
                            _ = channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbeds(embeds.Take(25).Select(x => x.Build())));
                            index += 25;
                        }
                    }
                }
                catch { }
                break;
            }
        }

        switch (e.LogEntry.LogLevel)
        {
            case CustomLogLevel.Fatal:
            {
                if (e.LogEntry.Message.ToLower().Contains("'not authenticated.'"))
                {
                    bot.status.DiscordDisconnections++;

                    if (bot.status.DiscordDisconnections >= 3)
                    {
                        _logger.LogRaised -= bot.LogHandler;
                        _ = bot.ExitApplication();
                    }
                    else
                    {
                        try
                        {
                            await bot.discordClient.ConnectAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogFatal("Failed to reconnect to discord", ex);
                            _logger.LogRaised -= bot.LogHandler;
                            _ = bot.ExitApplication();
                        }
                    }
                }
                break;
            }
            default:
                break;
        }
    }
}
