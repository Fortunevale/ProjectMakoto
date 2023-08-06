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
        this.Start();
    }

    private List<TaskInfo> TaskList = new();

    internal async void Start()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                Thread.Sleep(100);

                if (this.TaskList is null)
                {
                    Environment.Exit((int)ExitCodes.VitalTaskFailed);
                }

                if (this.TaskList.Count <= 0)
                {
                    continue;
                }

                for (var i = 0; i < this.TaskList.Count; i++)
                {
                    var b = this.TaskList[i];

                    if (b is null)
                    {
                        lock (this.TaskList) { _ = this.TaskList.Remove(b); }
                        i--;
                        continue;
                    }

                    if (!b.Task.IsCompleted)
                        continue;

                    lock (this.TaskList) { _ = this.TaskList.Remove(b); }
                    i--;

                    if (b.Task.IsCompletedSuccessfully)
                    {
                        _logger.LogTrace("Successfully executed Task:{Id} '{Uuid}' in {Elapsed}ms, Task Count now at {Count}.", 
                            b.Task.Id, b.GetName(), b.CreationTime.GetTimespanSince().TotalMilliseconds.ToString("N0", CultureInfo.CreateSpecificCulture("en-US")), this.TaskList.Count);

                        if (b.CustomData is SharedCommandContext sctx)
                        {
                            _logger.LogInfo("Successfully executed '{Prefix}{Name}' for '{User}' on '{Guild}'",
                                sctx?.Prefix,
                                sctx?.CommandName,
                                sctx?.User?.Id,
                                sctx?.Guild?.Id);
                        }
                        else if (b.CustomData is CommandContext cctx)
                        {
                            _logger.LogInfo("Successfully executed '{Prefix}{Name}' for '{User}' on '{Guild}'",
                                cctx?.Prefix,
                                cctx?.Command.Parent is not null ? $"{cctx.Command.Parent.Name} " : "" + cctx.Command.Name,
                                cctx?.User?.Id,
                                cctx?.Guild?.Id);
                        }
                        else if (b.CustomData is InteractionContext ictx)
                        {
                            _logger.LogInfo("Successfully executed '/{Name}' for '{User}' on '{Guild}'",
                                ictx?.FullCommandName,
                                ictx?.User?.Id,
                                ictx?.Guild?.Id);
                        }
                        else if (b.CustomData is ContextMenuContext cmctx)
                        {
                            _logger.LogInfo("Successfully executed '{Name}' for '{User}' on '{Guild}'",
                                cmctx?.FullCommandName,
                                cmctx?.User?.Id,
                                cmctx?.Guild?.Id);
                        }

                        continue;
                    }

                    if (b.CustomData is not null)
                    {
                        var Exception = (b.Task.Exception?.GetType() != typeof(AggregateException) ? b.Task.Exception : b.Task.Exception.InnerException);

                        if (Exception is DisCatSharp.Exceptions.BadRequestException badReq)
                        {
                            try
                            { _logger.LogError("Web Request: {Request}", (JsonConvert.SerializeObject(badReq?.WebRequest, Formatting.Indented).Replace("\\", ""))); }
                            catch { }
                            try
                            { _logger.LogError("Web Response: {Response}", badReq.WebResponse.Response.Replace("\\", "")); }
                            catch { }
                        }

                        if (b.CustomData is SharedCommandContext sctx)
                        {
                            _logger.LogError("Failed to execute '{Prefix}{Name}' for '{User}' on '{Guild}', Task Count now at {Count}.", b.Task.Exception,
                                sctx?.Prefix,
                                sctx?.CommandName,
                                sctx?.User?.Id,
                                sctx?.Guild?.Id,
                                this.TaskList.Count);

                            try
                            {
                                _ = sctx.BaseCommand.RespondOrEdit(new DiscordMessageBuilder()
                                    .WithContent(sctx.User.Mention)
                                    .AddEmbed(new DiscordEmbedBuilder()
                                        .WithDescription(sctx.BaseCommand.GetString(sctx.t.Commands.Common.Errors.UnhandledException, true, 
                                            new TVar("Message", $"```diff\n-{(Exception?.Message?.SanitizeForCode() ?? "No message captured.")}\n```"),
                                            new TVar("Timestamp", DateTime.UtcNow.AddSeconds(11).ToTimestamp())))
                                        .AsBotError(sctx)))
                                .ContinueWith(x =>
                                {
                                    if (!x.IsCompletedSuccessfully)
                                        return;

                                    _ = Task.Delay(10000).ContinueWith(_ =>
                                    {
                                        sctx.BaseCommand.DeleteOrInvalidate();
                                    });
                                });
                            }
                            catch (Exception ex) { _logger.LogError("Failed to notify user about unhandled exception.", ex); }
                        }
                        else
                        {
                            _logger.LogError("Task '{UUID}' failed to execute", b.Task.Exception, b.GetName());
                        }
                    }
                    else
                    {
                        _logger.LogError("Task '{UUID}' failed to execute", b.Task.Exception, b.GetName());
                    }

                    if (b.IsVital)
                    {
                        await Task.Delay(1000);
                        Environment.Exit((int)ExitCodes.VitalTaskFailed);
                        return;
                    }
                }
            }
        }).ContinueWith(async x =>
        {
            if (!x.IsCompletedSuccessfully)
            {
                _logger.LogError("TaskWatcher failed to execute", x.Exception);
                await Task.Delay(1000);
                Environment.Exit((int)ExitCodes.VitalTaskFailed);
                return;
            }
        });
    }

    internal TaskInfo AddToList(TaskInfo taskInfo)
    {
        _logger.LogTrace("Started Task:{uuid}, Task Count now at {Count}.", taskInfo.GetName(), this.TaskList.Count + 1);
        lock (this.TaskList) { this.TaskList.Add(taskInfo); }

        return taskInfo;
    }

    internal async static void LogHandler(Bot bot, object? sender, LogMessageEventArgs e, int depth = 0)
    {
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
                            await Task.Delay(10000);
                            await bot.DiscordClient.ConnectAsync();
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

                        var channel = bot.DiscordClient.Guilds[bot.status.LoadedConfig.Discord.DevelopmentGuild].GetChannel(bot.status.LoadedConfig.Channels.ExceptionLog);

                        if (channel is null)
                        {
                            if (depth > 10)
                            {
                                _logger.LogWarn("Could not notify of exception in channel");
                                return;
                            }

                            await Task.Delay(1000);
                            LogHandler(bot, sender, e, depth++);
                            return;
                        }

                        var template = new DiscordEmbedBuilder()
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
                                    _ = embed.WithDescription($"`{e.LogEntry.Message.SanitizeForCode()}`");
                                else
                                {
                                    embed.Title = "";
                                }

                                _ = embed.AddField(new DiscordEmbedField("Message", $"```{ex.Message.SanitizeForCode()}```"));
                                if (!ex.StackTrace.IsNullOrWhiteSpace())
                                {
                                    var regex = @"((?:(?:(?:[A-Z]:\\)|(?:\/))[^\\\/]*[\\\/]).*):line (\d{0,10})";
                                    var b = Regex.Matches(ex.StackTrace, regex);

                                    if (b.Count > 0)
                                    {
                                        _ = embed.AddField(new DiscordEmbedField("Stack Trace", $"```{Regex.Replace(ex.StackTrace, "in " + regex, "").Replace("   at ", "")}```".TruncateWithIndication(1024, "``` Stack Trace too long, please check logs.")));
                                        _ = embed.AddField(new DiscordEmbedField(b.Count > 1 ? "Files & Lines" : "File & Line", $"{string.Join("\n\n", b.Select(x => $"[`{x.Groups[1].Value[(x.Groups[1].Value.LastIndexOf("ProjectMakoto"))..].Replace("\\", "/")}`]" +
                                        $"(https://github.com/{bot.status.LoadedConfig.Secrets.Github.Username}/{bot.status.LoadedConfig.Secrets.Github.Repository}/blob/{bot.status.LoadedConfig.Secrets.Github.Branch ?? "main"}/{x.Groups[1].Value[(x.Groups[1].Value.LastIndexOf("ProjectMakoto"))..].Replace("\\", "/")}#L{x.Groups[2]}) at `Line {x.Groups[2]}`"))}".TruncateWithIndication(1024, "`")));
                                    }
                                    else
                                    {
                                        _ = embed.AddField(new DiscordEmbedField("Stack Trace", $"```{ex.StackTrace?.SanitizeForCode()}```".TruncateWithIndication(1024, "```")));
                                    }
                                }
                                else
                                {
                                    _ = embed.AddField(new DiscordEmbedField("Stack Trace", $"```No Stack Trace captured.```"));
                                }

                                _ = embed.AddField(new DiscordEmbedField("Source", $"`{ex.Source?.SanitizeForCode() ?? "No Source captured."}`".TruncateWithIndication(1024, "`"), true));
                                _ = embed.AddField(new DiscordEmbedField("Throwing Method", $"`{ex.TargetSite?.Name ?? "No Method captured"}` in `{ex.TargetSite?.DeclaringType?.Name ?? "No Type captured."}`".TruncateWithIndication(1024, "`"), true));
                                _ = embed.WithFooter(ex.HResult.ToString());

                                if ((ex.Data?.Keys?.Count ?? 0) > 0)
                                    _ = embed.AddFields(ex.Data.Keys.Cast<object>().ToDictionary(k => k.ToString(), v => ex.Data[v]).Select(x => new DiscordEmbedField(x.Key, x.Value.ToString().TruncateWithIndication(1024))));

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

                        var index = 0;

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
    }
}
