namespace Project_Ichigo.TaskWatcher;

internal static class TaskWatcherExtensions
{
    internal static void Add(this Task task, TaskWatcher watcher, CommandContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));
}
