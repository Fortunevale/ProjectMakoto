namespace ProjectIchigo.Database;

internal class DatabaseQueue
{
    internal DatabaseQueue(Bot _bot)
    {
        this._bot = _bot;

        _ = QueueHandler();
    }

    public Bot _bot { get; private set; }

    internal async Task QueueHandler()
    {
        Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    bool Removed = false;

                    for (int i = 0; i < Queue.Count; i++)
                    {
                        var obj = Queue[i];

                        if (obj is null || obj.Executed || obj.Failed)
                        {
                            Queue.Remove(obj);
                            Removed = true;
                            break;
                        }
                    }

                    if (Removed)
                        continue;

                    RequestQueue b = null;

                    for (int i = 0; i < Queue.Count; i++)
                    {
                        var obj = Queue[i];

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
                        Thread.Sleep(50);
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
                                    _logger.LogTrace($"Executing command on Database '{b.Command.Connection.Database}': '{b.Command.CommandText.TruncateWithIndication(100)}' with Priority {b.Priority}");
                                }
                                catch { }

                                b.Command.ExecuteNonQuery();

                                b.Executed = true;
                                break;
                            }
                            case DatabaseRequestType.Ping:
                            {
                                try
                                {
                                    _logger.LogTrace($"Executing ping on Database '{b.Connection.Database}' with Priority {b.Priority}");
                                }
                                catch { }

                                b.Connection.Ping();

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
                                _logger.LogWarn($"Connection with Database broken, attempting to reconnect..");

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
                            _logger.LogFatal($"Connection with Database broken, reconnecting failed.");
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
                FailCount++;

                if (FailCount > 20)
                {
                    _logger.LogFatal("Queue Handler failed 20 times, terminating application.");

                    _ = _bot.ExitApplication(true);
                    throw;
                }

                _ = QueueHandler();
                throw;
            }
        }).Add(_bot.watcher);
    }

    internal async Task RunCommand(MySqlCommand cmd, QueuePriority priority = QueuePriority.Normal, int depth = 0)
    {
        if (depth > 10)
            throw new Exception("Failed to run command.");

        RequestQueue value = new() { RequestType = DatabaseRequestType.Command, Command = cmd, Priority = priority };

        Queue.Add(value);
        try { Queue.Sort((a, b) => ((int)a?.Priority).CompareTo((int)b?.Priority)); } catch { }

        Stopwatch sw = new();

        while (true)
        {
            if (value.Executed || value.Failed || sw.ElapsedMilliseconds > 30000)
                break;

            Thread.Sleep(50);
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

        Queue.Add(value);
        try { Queue.Sort((a, b) => ((int)a?.Priority).CompareTo((int)b?.Priority)); } catch { }

        Stopwatch sw = new();

        while (true)
        {
            if (value.Executed || value.Failed || sw.ElapsedMilliseconds > 30000)
                break;

            Thread.Sleep(50);
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

    internal class RequestQueue
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
