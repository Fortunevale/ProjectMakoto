// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;

public sealed class ThreadJoinClient
{
    internal ThreadJoinClient() { }

    internal static ThreadJoinClient Initialize()
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
