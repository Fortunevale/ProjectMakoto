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
    public static TaskInfo Add(this Task task, Bot bot)
        => bot.watcher.AddToList(new TaskInfo(task));

    /// <summary>
    /// Add Task to Watcher with Custom Data
    /// </summary>
    /// <param name="task">The task</param>
    /// <param name="watcher">The current Watcher Instance</param>
    /// <param name="customData">The CustomData to attach</param>
    public static TaskInfo Add(this Task task, Bot bot, object? customData)
        => bot.watcher.AddToList(new TaskInfo(task, customData));

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
