// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class InteractionResult<T>
{
    public InteractionResult(T result)
    {
        this.Result = result;
    }

    public InteractionResult(Exception exception)
    {
        this.Exception = exception;
    }

    public T Result { get; set; }

    public bool Failed { get { return this.TimedOut || this.Cancelled; } }

    public bool TimedOut { get { return (this.Exception is not null && this.Exception.GetType() == typeof(TimedOutException)); } }

    public bool Cancelled { get { return (this.Exception is not null && this.Exception.GetType() == typeof(CancelException)); } }

    public bool Errored { get { return this.Exception is not null; } }

    public Exception Exception { get; set; }
}
