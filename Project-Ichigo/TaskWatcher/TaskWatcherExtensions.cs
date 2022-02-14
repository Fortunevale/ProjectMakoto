namespace Project_Ichigo.TaskWatcher;

internal static class TaskWatcherExtensions
{
    internal static void Add(this Task task, TaskWatcher watcher, CommandContext ctx)
    {
        watcher.AddToList(new TaskInfo
        {
            task = task,
            ctx = ctx,
            uuid = Guid.NewGuid().ToString()
        });
    }
}
