// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMakoto.Plugins;
public sealed class PluginLoggerClient
{
    internal PluginLoggerClient(LoggerClient client, BasePlugin parent)
    {
        this._client = client;
        this.Parent = parent;
    }

    private LoggerClient _client;

    private BasePlugin Parent;

    /// <inheritdoc cref="LoggerClient.LogTrace(string, Exception?, object[])"/>
    public void LogTrace(string message, Exception? exception = null, params object[] args)
        => _client.LogTrace(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogTrace(string, object[])"/>
    public void LogTrace(string message, params object[] args)
        => _client.LogTrace(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogDebug(string, Exception?, object[])"/>
    public void LogDebug(string message, Exception? exception = null, params object[] args)
        => _client.LogDebug(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogDebug(string, object[])"/>
    public void LogDebug(string message, params object[] args)
        => _client.LogDebug(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogInfo(string, Exception?, object[])"/>
    public void LogInfo(string message, Exception? exception = null, params object[] args)
        => _client.LogInfo(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogInfo(string, object[])"/>
    public void LogInfo(string message, params object[] args)
        => _client.LogInfo(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogWarn(string, Exception?, object[])"/>
    public void LogWarn(string message, Exception? exception = null, params object[] args)
        => _client.LogWarn(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogWarn(string, object[])"/>
    public void LogWarn(string message, params object[] args)
        => _client.LogWarn(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogError(string, Exception?, object[])"/>
    public void LogError(string message, Exception? exception = null, params object[] args)
        => _client.LogError(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogError(string, object[])"/>
    public void LogError(string message, params object[] args)
        => _client.LogError(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogFatal(string, Exception?, object[])"/>
    public void LogFatal(string message, Exception? exception = null, params object[] args)
        => _client.LogFatal(message.Insert(0, "[{Plugin}] "), exception, args.Prepend(Parent.Name).ToArray());

    /// <inheritdoc cref="LoggerClient.LogFatal(string, object[])"/>
    public void LogFatal(string message, params object[] args)
        => _client.LogFatal(message.Insert(0, "[{Plugin}] "), args.Prepend(Parent.Name).ToArray());
}
