// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class TaskInfo
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

    public string uuid { get; private set; } = Guid.NewGuid().ToString();
    public CommandContext? CommandContext { get; private set; } = null;
    public InteractionContext? InteractionContext { get; private set; } = null;
    public SharedCommandContext? SharedCommandContext { get; private set; } = null;
    public ContextMenuContext? ContextMenuContext { get; private set; } = null;
    public Task task { get; private set; }
    public DateTime CreationTimestamp { get; private set; } = DateTime.UtcNow;
    public bool IsVital { get; internal set; } = false;
}
