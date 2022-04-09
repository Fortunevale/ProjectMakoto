namespace Project_Ichigo.Database;

internal class DatabaseQueue
{
    internal DatabaseQueue()
    {
        _ = QueueHandler();
    }

    internal async Task QueueHandler()
    {
        _ = Task.Run(async () =>
        {
            string lastResolve = "";

            while (true)
            {
                if (Queue.Count == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (Queue.ContainsKey(lastResolve))
                    Queue.Remove(lastResolve);

                if (Queue.Count == 0)
                    continue;

                var b = Queue.First();

                try
                {
                    b.Value.Command.ExecuteNonQuery();

                    Queue[b.Key].Executed = true;
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
                    lastResolve = b.Key;
                    Thread.Sleep(1000);
                }

                GC.KeepAlive(b);
                GC.KeepAlive(b.Key);
                GC.KeepAlive(b.Value);
                GC.KeepAlive(b.Value.Failed);
                GC.KeepAlive(b.Value.Command);
                GC.KeepAlive(b.Value.Executed);
                GC.KeepAlive(b.Value.Exception);
            }
        });
    }

    internal async Task RunCommand(MySqlCommand cmd)
    {
        string key = Guid.NewGuid().ToString();

        Queue.Add(key, new RequestQueue { Command = cmd });

        while (Queue.ContainsKey(key) && !Queue[key].Executed && !Queue[key].Failed)
            Thread.Sleep(50);

        var response = Queue[key];
        Queue.Remove(key);

        if (response.Executed)
            return;

        if (response.Failed)
            throw response.Exception;

        throw new Exception("This exception should be impossible to get.");
    }

    internal int QueueCount()
    {
        return this.Queue.Count;
    }

    readonly Dictionary<string, RequestQueue> Queue = new();

    internal class RequestQueue
    {
        public MySqlCommand Command { get; set; }
        public bool Executed { get; set; }
        public bool Failed { get; set; }
        public Exception Exception { get; set; }
    }
}
