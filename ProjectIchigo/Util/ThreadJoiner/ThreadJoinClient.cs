namespace ProjectIchigo.Util;

internal class ThreadJoinClient
{
    public static ThreadJoinClient Initialize()
    {
        ThreadJoinClient threadJoinClient = new();
        _ = threadJoinClient.QueueHandler();
        return threadJoinClient;
    }

    internal readonly Dictionary<ulong, DiscordThreadChannel> Queue = new();

    private async Task QueueHandler()
    {
        while (true)
        {
            if (Queue.Count == 0)
            {
                await Task.Delay(100);
                continue;
            }

            var b = Queue.First();

            try
            {
                await b.Value.JoinAsync();

                lock (Queue)
                {
                    Queue.Remove(b.Key);
                }
            }
            finally
            {
                await Task.Delay(1000);
            }
        }
    }

    public async Task JoinThread(DiscordThreadChannel channel)
    {
        lock (Queue)
        {
            if (Queue.ContainsKey(channel.Id))
                return;

            Queue.Add(channel.Id, channel);
            return;
        }
    }
}
