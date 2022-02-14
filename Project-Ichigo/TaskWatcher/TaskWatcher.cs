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

                if (b.task.IsCompletedSuccessfully)
                {
                    tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
                    continue;
                }

                var ctx = b.ctx;

                LogError($"Failed to execute '{ctx.Prefix}{ctx.Command.Name}{(ctx.RawArgumentString == "" ? "" : $" {ctx.RawArgumentString}")}' for {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id}) in #{ctx.Channel.Name}  on '{ctx.Guild.Name}' ({ctx.Guild.Id}): {b.task.Exception}");
                tasks.RemoveAt(tasks.FindIndex(x => x.uuid == b.uuid));
            }

            await Task.Delay(1000);
        }
    }

    internal async void AddToList(TaskInfo taskInfo) => tasks.Add(taskInfo);

}
