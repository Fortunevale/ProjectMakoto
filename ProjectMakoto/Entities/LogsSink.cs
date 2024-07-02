// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using Serilog.Core;
using Serilog.Events;

namespace ProjectMakoto.Entities;
public class LogsSink(Bot bot) : ILogEventSink
{
    /// <summary>
    /// Emit the provided log event to the sink.
    /// </summary>
    /// <param name="logEvent">The log event to write</param>
    public void Emit(LogEvent logEvent)
    {
        bot.Watcher.LogHandler(bot, null, logEvent);
    }
}