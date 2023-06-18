// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands.InviteTrackerCommand;

internal sealed class InviteTrackerCommandAbstractions
{
    internal static string GetCurrentConfiguration(SharedCommandContext ctx)
    {
        return $"{"📲".UnicodeToEmoji()} `{ctx.BaseCommand.GetString(ctx.BaseCommand.t.Commands.Config.InviteTracker.InviteTrackerEnabled)}`: {ctx.DbGuild.InviteTracker.Enabled.ToEmote(ctx.Bot)}";
    }
}
