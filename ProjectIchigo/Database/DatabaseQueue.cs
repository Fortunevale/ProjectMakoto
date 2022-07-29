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
            while (true)
            {
                KeyValuePair<string, RequestQueue> b;

                try
                {
                    if (!Queue.Any(x => !x.Value.Executed && !x.Value.Failed))
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    b = Queue.OrderBy(x => (int)x.Value?.Priority).First(x => !x.Value.Executed && !x.Value.Failed);
                }
                catch (Exception ex) 
                { 
                    _logger.LogError("Failed to get ordered Queue", ex);

                    Queue = new();

                    continue; 
                }

                if (b.Value is null)
                    continue;

                try
                {
                    switch (b.Value.RequestType)
                    {
                        case DatabaseRequestType.Command:
                        {
                            try
                            {
                                _logger.LogTrace($"Executing command on Database '{b.Value.Command.Connection.Database}': '{b.Value.Command.CommandText.TruncateWithIndication(100)}'");
                            }
                            catch { }

                            b.Value.Command.ExecuteNonQuery();

                            Queue[b.Key].Executed = true;
                            break;
                        }
                        case DatabaseRequestType.Ping:
                        {
                            b.Value.Connection.Ping();

                            Queue[b.Key].Executed = true;
                            break;
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    _logger.LogError($"An exception occured while trying to execute a mysql command", ex);
                }
                catch (Exception ex)
                {
                    Queue[b.Key].Failed = true;
                    Queue[b.Key].Exception = ex;
                }
                finally
                {
                    Thread.Sleep(10);
                }
            }
        }).Add(_bot._watcher);
    }

    internal async Task RunCommand(MySqlCommand cmd, QueuePriority priority = QueuePriority.Normal, int depth = 0)
    {
        if (depth > 10)
            throw new Exception("Failed to run command.");

        string key = Guid.NewGuid().ToString();

        RequestQueue value;

        try
        {
            value = new RequestQueue { RequestType = DatabaseRequestType.Command, Command = cmd, Priority = priority };
            Queue.Add(key, value);
        }
        catch (Exception)
        {
            if (Queue is null)
            {
                Queue = new();

                await Task.Delay(1000);

                await RunCommand(cmd, priority, depth + 1);
                return;
            }
            throw;
        }

        while (Queue.ContainsKey(key) && !value.Executed && !value.Failed)
            Thread.Sleep(1);

        Queue.Remove(key);

        if (value.Executed)
            return;
        else if (value.Failed)
            throw value.Exception ?? new Exception("The command execution failed but there no exception was stored");
        else
            throw new Exception("The command was not processed.");

        throw new Exception("This exception should be impossible to get.");
    }

    internal async Task<bool> RunPing(MySqlConnection conn, int depth = 0)
    {
        if (depth > 10)
            throw new Exception("Failed to run ping.");

        string key = Guid.NewGuid().ToString();

        RequestQueue value;

        try
        {
            value = new RequestQueue { RequestType = DatabaseRequestType.Ping, Connection = conn, Priority = QueuePriority.Low };
            Queue.Add(key, value);
        }
        catch (Exception)
        {
            if (Queue is null)
            {
                Queue = new();

                await Task.Delay(1000);

                return await RunPing(conn, depth + 1);
            }
            throw;
        }

        while (Queue.ContainsKey(key) && !value.Executed && !value.Failed)
            Thread.Sleep(50);

        Queue.Remove(key);

        if (value.Executed)
            return true;
        else if (value.Failed)
            return false;
        else
            throw new Exception("The ping was not processed.");

        throw new Exception("This exception should be impossible to get.");
    }

    internal int QueueCount => this.Queue.Count;

    private Dictionary<string, RequestQueue> Queue = new();

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
