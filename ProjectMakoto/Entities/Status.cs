// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal class Status
{
    internal DateTime startupTime { get; set; } = DateTime.UtcNow;

    internal bool DiscordInitialized { get; set; } = false;
    internal bool DiscordGuildDownloadCompleted { get; set; } = false;
    internal bool DiscordCommandsRegistered { get; set; } = false;
    internal bool LavalinkInitialized { get; set; } = false;
    internal bool DatabaseInitialized { get; set; } = false;
    internal bool DatabaseInitialLoadCompleted { get; set; } = false;

    internal ulong TeamOwner { get; set; } = new();
    internal List<ulong> TeamMembers { get; set; } = new();

    internal long DiscordDisconnections = 0;

    internal Config LoadedConfig { get; set; }

    #region Legacy

    internal string DevelopmentServerInvite
    {
        get
        {
            if (LoadedConfig.SupportServerInvite.IsNullOrWhiteSpace())
                return "Invite not set.";

            return LoadedConfig.SupportServerInvite;
        }
    }

    #endregion
}
