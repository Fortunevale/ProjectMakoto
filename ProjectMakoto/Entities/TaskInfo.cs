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
        this.Task = task;
    }

    internal TaskInfo(Task task, object? customData = null)
    {
        this.CustomData = customData;
        this.Task = task;
    }
    public string Uuid { get; private set; } = Guid.NewGuid().ToString();
    public Task Task { get; private set; }
    public DateTime CreationTime { get; private set; } = DateTime.UtcNow;
    public bool IsVital { get; internal set; } = false;

    public string CallingMethod { get; init; } = string.Empty;
    public string CallingFile { get; init; } = string.Empty;
    public int CallingLine { get; init; } = -1;

    public object? CustomData { get; internal set;} = null;

    public string GetName()
        => $"{this.Uuid}; F:{this.CallingFile ?? "-"}:{this.CallingLine} (M:{this.CallingMethod ?? "-"})";
}
