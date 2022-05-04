namespace Project_Ichigo.TaskWatcher;

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

                var ctx = b.ctx;

                if (b.task.IsCompletedSuccessfully)
                {
                    LogDebug2($"Successfully executed task:{b.task.Id} '{b.uuid}' in {b.CreationTimestamp.GetTimespanSince().TotalMilliseconds:N0}ms");

                    if (ctx is not null)
                        LogInfo($"Successfully executed Command in {b.CreationTimestamp.GetTimespanSince().TotalMilliseconds:N0}ms for '{ctx.Prefix}{ctx.Command.Name}{(string.IsNullOrWhiteSpace(ctx.RawArgumentString) ? "" : $" {ctx.RawArgumentString}")}' for {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id}) in #{ctx.Channel.Name}  on '{ctx.Guild.Name}' ({ctx.Guild.Id})");

                    tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
                    continue;
                }

                if (ctx != null)
                    LogError($"Failed to execute '{ctx.Prefix}{ctx.Command.Name}{(string.IsNullOrWhiteSpace(ctx.RawArgumentString) ? "" : $" {ctx.RawArgumentString}")}' for {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id}) in #{ctx.Channel.Name}  on '{ctx.Guild.Name}' ({ctx.Guild.Id})", b.task.Exception);
                else
                    LogError($"A non-command task failed to execute: {b.task.Exception}");

                if (b.task.Exception.InnerException.GetType() == typeof(DisCatSharp.Exceptions.BadRequestException))
                {
                    LogError($"WebRequestUrl: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest.Url}\n\n" +
                            $"WebRequest: {JsonConvert.SerializeObject(((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebRequest, Formatting.Indented).Replace("\\", "")}\n\n" +
                            $"WebResponse: {((DisCatSharp.Exceptions.BadRequestException)b.task.Exception.InnerException).WebResponse.Response}");
                }

                if (ctx != null)
                    try
                    {
                        await ctx.Channel.SendMessageAsync($"{ctx.User.Mention}\n:warning: `I'm sorry but an unhandled exception occured while trying to execute your command.`\n\n" +
                                                           $"```csharp\n" +
                                                           $"{b.task.Exception}" +
                                                           $"\n```");
                    }
                    catch { }

                tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
            }

            await Task.Delay(1000);
        }
    }

    internal async void AddToList(TaskInfo taskInfo) => tasks.Add(taskInfo);

}
