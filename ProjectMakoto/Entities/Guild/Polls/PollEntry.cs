// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class PollEntry
{
    public string PollText { get; set; }

    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public string EndEarlyUUID { get; set; }

    public string SelectUUID { get; set; }

    public DateTime DueTime { get; set; }

    public Dictionary<string, string> Options { get; set; }

    public Dictionary<ulong, List<string>> Votes { get; set; }
}
