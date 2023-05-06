// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal class TaskInfo
{
    internal TaskInfo(Task task)
    {
        this.task = task;
    }
    
    internal TaskInfo(Task task, CommandContext ctx = null)
    {
        this.CommandContext = ctx;
        this.task = task;
    }

    internal TaskInfo(Task task, InteractionContext ctx = null)
    {
        this.InteractionContext = ctx;
        this.task = task;
    }
    
    internal TaskInfo(Task task, SharedCommandContext ctx = null)
    {
        this.SharedCommandContext = ctx;
        this.task = task;
    }
    
    internal TaskInfo(Task task, ContextMenuContext ctx = null)
    {
        this.ContextMenuContext = ctx;
        this.task = task;
    }

    internal string uuid { get; private set; } = Guid.NewGuid().ToString();
    internal CommandContext? CommandContext { get; private set; } = null;
    internal InteractionContext? InteractionContext { get; private set; } = null;
    internal SharedCommandContext? SharedCommandContext { get; private set; } = null;
    internal ContextMenuContext? ContextMenuContext { get; private set; } = null;
    internal Task task { get; private set; }
    internal DateTime CreationTimestamp { get; private set; } = DateTime.UtcNow;
}
