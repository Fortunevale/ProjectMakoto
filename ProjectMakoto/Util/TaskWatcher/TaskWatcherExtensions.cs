// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public static class TaskWatcherExtensions
{
    /// <summary>
    /// Add Task to Watcher without any Context
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    public static TaskInfo Add(this Task task, TaskWatcher watcher) => watcher.AddToList(new TaskInfo(task));

    /// <summary>
    /// Add Task to Watcher with CommandContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The CommandContext</param>
    public static TaskInfo Add(this Task task, TaskWatcher watcher, CommandContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));

    /// <summary>
    /// Add Task to Watcher with InteractionContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The InteractionContext</param>
    public static TaskInfo Add(this Task task, TaskWatcher watcher, InteractionContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));

    /// <summary>
    /// Add Task to Watcher with InteractionContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The InteractionContext</param>
    public static TaskInfo Add(this Task task, TaskWatcher watcher, ContextMenuContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));


    /// <summary>
    /// Add Task to Watcher with SharedCommandContext
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="ctx">The InteractionContext</param>
    public static TaskInfo Add(this Task task, TaskWatcher watcher, SharedCommandContext ctx = null) => watcher.AddToList(new TaskInfo(task, ctx));

    /// <summary>
    /// Mark this Task as vital to the operation of this program. Program will exit if failed.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    internal static TaskInfo IsVital(this TaskInfo info)
    {
        info.IsVital = true;
        return info;
    }
}
