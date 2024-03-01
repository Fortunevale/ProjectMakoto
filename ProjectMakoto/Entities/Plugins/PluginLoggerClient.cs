// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Plugins;
public sealed class PluginLoggerClient
{
    internal PluginLoggerClient(ILogger client, BasePlugin parent)
    {
        this._client = client;
        this.Parent = parent;
    }

    private ILogger _client;

    private BasePlugin Parent;

    /// <inheritdoc cref="Serilog.Log.Verbose(Exception?, string, object?[]?)"/>
    public void LogTrace(string message, Exception? exception = null, params object[] args)
        => this._client.Verbose(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Verbose(Exception?, string, object?[]?)"/>
    public void LogTrace(Exception? exception, string message, params object[] args)
        => this._client.Verbose(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Verbose(string, object?[]?)"/>
    public void LogTrace(string message, params object[] args)
        => this._client.Verbose(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());


    /// <inheritdoc cref="Serilog.Log.Debug(Exception?, string, object?[]?)"/>
    public void LogDebug(string message, Exception? exception = null, params object[] args)
        => this._client.Debug(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Debug(Exception?, string, object?[]?)"/>
    public void LogDebug(Exception? exception, string message, params object[] args)
        => this._client.Debug(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Debug(string, object?[]?)"/>
    public void LogDebug(string message, params object[] args)
        => this._client.Debug(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());


    /// <inheritdoc cref="Serilog.Log.Information(Exception?, string, object?[]?)"/>
    public void LogInfo(string message, Exception? exception = null, params object[] args)
        => this._client.Information(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());
    
    /// <inheritdoc cref="Serilog.Log.Information(Exception?, string, object?[]?)"/>
    public void LogInfo(Exception? exception, string message, params object[] args)
        => this._client.Information(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Information(string, object?[]?)"/>
    public void LogInfo(string message, params object[] args)
        => this._client.Information(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());


    /// <inheritdoc cref="Serilog.Log.Warning(Exception?, string, object?[]?)"/>
    public void LogWarn(string message, Exception? exception = null, params object[] args)
        => this._client.Warning(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Warning(Exception?, string, object?[]?)"/>
    public void LogWarn(Exception? exception, string message, params object[] args)
        => this._client.Warning(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Warning(string, object?[]?)"/>
    public void LogWarn(string message, params object[] args)
        => this._client.Warning(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());


    /// <inheritdoc cref="Serilog.Log.Error(Exception?, string, object?[]?)"/>
    public void LogError(string message, Exception? exception = null, params object[] args)
        => this._client.Error(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Error(Exception?, string, object?[]?)"/>
    public void LogError(Exception? exception, string message, params object[] args)
        => this._client.Error(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Error(string, object?[]?)"/>
    public void LogError(string message, params object[] args)
        => this._client.Error(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());


    /// <inheritdoc cref="Serilog.Log.Fatal(Exception?, string, object?[]?)"/>
    public void LogFatal(string message, Exception? exception = null, params object[] args)
        => this._client.Fatal(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());
    
    /// <inheritdoc cref="Serilog.Log.Fatal(Exception?, string, object?[]?)"/>
    public void LogFatal(Exception? exception, string message, params object[] args)
        => this._client.Fatal(exception, message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());

    /// <inheritdoc cref="Serilog.Log.Fatal(string, object?[]?)"/>
    public void LogFatal(string message, params object[] args)
        => this._client.Fatal(message.Insert(0, "[{Plugin}] "), args.Prepend(this.Parent.Name).ToArray());
}
