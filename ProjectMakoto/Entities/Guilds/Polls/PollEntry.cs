// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities.Guilds;

public sealed class PollEntry
{
    [JsonIgnore]
    public Bot Bot { get; set; }

    [JsonIgnore]
    public PollSettings Parent { get; set; }

    public string PollText { get; set; }

    public ulong ChannelId { get; set; }

    public ulong MessageId { get; set; }

    public string EndEarlyUUID { get; set; }

    public string SelectUUID { get; set; }

    public DateTime DueTime { get; set; }

    public Dictionary<string, string> Options { get; set; }

    private Vote[] _Votes { get; set; }
    public Vote[] Votes
    {
        get => this._Votes;
        set
        {
            this._Votes = value;
            this.Update();
        }
    }

    public record Vote(ulong Voter, string[] SelectedVotes);


    void Update()
    {
        if (this.Bot is null || this.Parent is null)
            return;

        this.Parent.RunningPolls = this.Parent.RunningPolls.Update(x => x.SelectUUID.ToString(), this);
    }
}
