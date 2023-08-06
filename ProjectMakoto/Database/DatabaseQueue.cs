// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Database;

internal sealed class DatabaseQueue : RequiresBotReference
{
    internal DatabaseQueue(Bot _bot) : base(_bot)
    {
        _ = this.QueueHandler();
    }

    internal async Task QueueHandler()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var Removed = false;

                    for (var i = 0; i < this.Queue.Count; i++)
                    {
                        var obj = this.Queue[i];

                        if (obj is null || obj.Executed || obj.Failed)
                        {
                            _ = this.Queue.Remove(obj);
                            Removed = true;
                            break;
                        }
                    }

                    if (Removed)
                        continue;

                    RequestQueue b = null;

                    for (var i = 0; i < this.Queue.Count; i++)
                    {
                        var obj = this.Queue[i];

                        if (obj is null)
                            continue;

                        if (!obj.Executed && !obj.Failed)
                        {
                            b = obj;
                            break;
                        }
                    }

                    if (b is null)
                    {
                        await Task.Delay(50);
                        continue;
                    }

                    try
                    {
                        switch (b.RequestType)
                        {
                            case DatabaseRequestType.Command:
                            {
                                try
                                {
                                    _logger.LogTrace("Executing command on Database '{Database}': '{Command}' with Priority {Priority}", b.Command.Connection.Database, b.Command.CommandText.TruncateWithIndication(100), b.Priority);
                                }
                                catch { }

                                _ = b.Command.ExecuteNonQuery();

                                b.Executed = true;
                                break;
                            }
                            case DatabaseRequestType.Ping:
                            {
                                try
                                {
                                    _logger.LogTrace("Executing ping on Database '{Database}' with Priority {Priority}", b.Connection.Database, b.Priority);
                                }
                                catch { }

                                _ = b.Connection.Ping();

                                b.Executed = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToLower().Contains("open datareader"))
                            continue;

                        _logger.LogError(b.Command.CommandText);

                        try
                        {
                            if (ex.Message.ToLower().Contains("connection must be valid and open."))
                            {
                                _logger.LogWarn("Connection with Database broken, attempting to reconnect..");

                                switch (b.RequestType)
                                {
                                    case DatabaseRequestType.Command:
                                    {
                                        b.Command.Connection.Open();
                                        break;
                                    }
                                    case DatabaseRequestType.Ping:
                                    {
                                        b.Connection.Open();
                                        break;
                                    }
                                }
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            _logger.LogFatal("Connection with Database broken, reconnecting failed.");
                        }

                        b.Failed = true;
                        b.Exception = ex;
                    }
                    finally
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception)
            {
                this.FailCount++;

                if (this.FailCount > 20)
                {
                    _logger.LogFatal("Queue Handler failed 20 times, terminating application.");

                    _ = this.Bot.ExitApplication(true);
                    throw;
                }

                _ = this.QueueHandler();
                throw;
            }
        }).Add(this.Bot);
    }

    internal async Task RunCommand(MySqlCommand cmd, QueuePriority priority = QueuePriority.Normal, int depth = 0)
    {
        if (depth > 10)
            throw new Exception("Failed to run command.");

        RequestQueue value = new() { RequestType = DatabaseRequestType.Command, Command = cmd, Priority = priority };

        this.Queue.Add(value);
        try
        { this.Queue.Sort((a, b) => ((int)a?.Priority).CompareTo((int)b?.Priority)); }
        catch { }

        Stopwatch sw = new();

        while (true)
        {
            if (value.Executed || value.Failed || sw.ElapsedMilliseconds > 30000)
                break;

            await Task.Delay(50);
        }

        sw.Stop();

        if (value.Executed)
            return;
        else if (value.Failed)
            throw value.Exception ?? new Exception("The command execution failed but there no exception was stored");

        throw new Exception("Command execution timed out.");
    }

    internal async Task<bool> RunPing(MySqlConnection conn, int depth = 0)
    {
        if (depth > 10)
            throw new Exception("Failed to run ping.");

        RequestQueue value = new() { RequestType = DatabaseRequestType.Ping, Connection = conn, Priority = QueuePriority.Low };

        this.Queue.Add(value);
        try
        { this.Queue.Sort((a, b) => ((int)a?.Priority).CompareTo((int)b?.Priority)); }
        catch { }

        Stopwatch sw = new();

        while (true)
        {
            if (value.Executed || value.Failed || sw.ElapsedMilliseconds > 30000)
                break;

            await Task.Delay(50);
        }

        sw.Stop();

        if (value.Executed)
            return true;
        else if (value.Failed)
            return false;

        throw new Exception("Command execution timed out.");
    }

    internal int QueueCount => this.Queue.Count;

    internal List<RequestQueue> Queue = new();

    int FailCount = 0;

    internal sealed class RequestQueue
    {
        public DatabaseRequestType RequestType { get; set; }
        public QueuePriority Priority { get; set; } = QueuePriority.Normal;
        public MySqlConnection Connection { get; set; }
        public MySqlCommand Command { get; set; }
        public bool Executed { get; set; } = false;
        public bool Failed { get; set; } = false;
        public Exception Exception { get; set; }
    }
}
