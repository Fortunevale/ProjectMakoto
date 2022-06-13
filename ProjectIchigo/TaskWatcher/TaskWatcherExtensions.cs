namespace ProjectIchigo.TaskWatcher;

internal static class TaskWatcherExtensions
{
    /// <summary>
    /// Add Task to Watcher without any Context
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    internal static void Add(this Task task, TaskWatcher watcher) => watcher.AddToList(new TaskInfo(task));

    /// <summary>
    /// Add Task to Watcher with CommandContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The CommandContext</param>
    internal static void Add(this Task task, TaskWatcher watcher, CommandContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));

    /// <summary>
    /// Add Task to Watcher with InteractionContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The InteractionContext</param>
    internal static void Add(this Task task, TaskWatcher watcher, InteractionContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));
}
