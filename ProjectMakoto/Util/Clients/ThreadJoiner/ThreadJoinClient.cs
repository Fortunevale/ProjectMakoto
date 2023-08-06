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
    internal ThreadJoinClient()
    {
        _ = this.QueueHandler();
    }

    ~ThreadJoinClient()
    {
        this._disposed = true;
    }

    bool _disposed = false;

    internal readonly Dictionary<ulong, DiscordThreadChannel> Queue = new();

    private async Task QueueHandler()
    {
        while (!this._disposed)
        {
            if (this.Queue.Count == 0)
            {
                await Task.Delay(100);
                continue;
            }

            var b = this.Queue.First();

            try
            {
                await b.Value.JoinAsync();

                lock (this.Queue)
                {
                    _ = this.Queue.Remove(b.Key);
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
        lock (this.Queue)
        {
            if (this.Queue.ContainsKey(channel.Id))
                return;

            this.Queue.Add(channel.Id, channel);
            return;
        }
    }
}
