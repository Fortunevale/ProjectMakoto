// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

public sealed class Status
{
    internal Status() { }

    public DateTime startupTime { get; internal set; } = DateTime.UtcNow;

    internal bool MigrationRequired { get; set; } = false;
    public string RunningVersion { get; internal set; }

    public bool DiscordInitialized { get; internal set; } = false;
    public bool DiscordGuildDownloadCompleted { get; internal set; } = false;
    public bool DiscordCommandsRegistered { get; internal set; } = false;
    public bool LavalinkInitialized { get; internal set; } = false;

    public ulong TeamOwner { get; internal set; } = new();
    public IReadOnlyList<ulong> TeamMembers
        => this._TeamMembers.AsReadOnly();
    internal List<ulong> _TeamMembers { get; set; } = new();

    internal long DiscordDisconnections = 0;

    internal Config LoadedConfig { get; set; }

    #region Legacy

    internal string DevelopmentServerInvite
    {
        get
        {
            return this.LoadedConfig.SupportServerInvite.IsNullOrWhiteSpace() ? "Invite not set." : this.LoadedConfig.SupportServerInvite;
        }
    }

    #endregion
}
