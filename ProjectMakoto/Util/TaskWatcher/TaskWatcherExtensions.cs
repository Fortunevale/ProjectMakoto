// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Runtime.CompilerServices;

namespace ProjectMakoto.Util;

public static class TaskWatcherExtensions
{
    /// <summary>
    /// Add Task to Watcher without any Context
    /// </summary>
    public static TaskInfo Add(this Task task, Bot bot, [CallerMemberName] string callingMember = "", [CallerFilePath] string callingFile = "", [CallerLineNumber] int callingLine = -1)
        => bot.Watcher.AddToList(new TaskInfo(task)
        {
            CallingMethod = callingMember,
            CallingFile = callingFile,
            CallingLine = callingLine
        });

    /// <summary>
    /// Add Task to Watcher with Custom Data
    /// </summary>
    public static TaskInfo Add(this Task task, Bot bot, object? customData, [CallerMemberName] string callingMember = "", [CallerFilePath] string callingFile = "", [CallerLineNumber] int callingLine = -1)
        => bot.Watcher.AddToList(new TaskInfo(task, customData)
        {
            CallingMethod = callingMember,
            CallingFile = callingFile,
            CallingLine = callingLine
        });

    /// <summary>
    /// Mark this Task as vital to the operation of this program. Program will exit if failed.
    /// </summary>
    internal static TaskInfo IsVital(this TaskInfo info)
    {
        info.IsVital = true;
        return info;
    }
}
