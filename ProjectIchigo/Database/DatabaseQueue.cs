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
        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (Queue.Count == 0 || !Queue.Any(x => !x.Value.Executed && !x.Value.Failed))
                {
                    Thread.Sleep(100);
                    continue;
                }

                var b = Queue.First(x => !x.Value.Executed && !x.Value.Failed);

                if (b.Value is null)
                    continue;

                try
                {
                    switch (b.Value.RequestType)
                    {
                        case DatabaseRequestType.Command:
                        {
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
                    LogError($"An exception occured while trying to execute a mysql command", ex);
                }
                catch (Exception ex)
                {
                    Queue[b.Key].Failed = true;
                    Queue[b.Key].Exception = ex;
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        });
    }

    internal async Task RunCommand(MySqlCommand cmd)
    {
        string key = Guid.NewGuid().ToString();

        Queue.Add(key, new RequestQueue { RequestType = DatabaseRequestType.Command, Command = cmd });

        while (Queue.ContainsKey(key) && !Queue[key].Executed && !Queue[key].Failed)
            Thread.Sleep(1);

        var response = Queue[key];
        Queue.Remove(key);

        if (response.Executed)
            return;

        if (response.Failed)
            throw response.Exception ?? new Exception("The command execution failed but there no exception was stored");

        throw new Exception("This exception should be impossible to get.");
    }

    internal async Task<bool> RunPing(MySqlConnection conn)
    {
        string key = Guid.NewGuid().ToString();

        Queue.Add(key, new RequestQueue { RequestType = DatabaseRequestType.Ping, Connection = conn });

        while (Queue.ContainsKey(key) && !Queue[key].Executed && !Queue[key].Failed)
            Thread.Sleep(50);

        var response = Queue[key];
        Queue.Remove(key);

        if (response.Executed)
            return true;

        if (response.Failed)
            return false;

        throw new Exception("This exception should be impossible to get.");
    }

    internal int QueueCount()
    {
        return this.Queue.Count;
    }

    readonly Dictionary<string, RequestQueue> Queue = new();

    internal class RequestQueue
    {
        public DatabaseRequestType RequestType { get; set; }
        public MySqlConnection Connection { get; set; }
        public MySqlCommand Command { get; set; }
        public bool Executed { get; set; }
        public bool Failed { get; set; }
        public Exception Exception { get; set; }
    }
}
