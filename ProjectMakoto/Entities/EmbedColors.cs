// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal class EmbedColors
{
    public static DiscordColor Error => new("dd2e44");
    public static DiscordColor StrongPunishment => DiscordColor.DarkRed;
    public static DiscordColor LightPunishment => DiscordColor.Red;
    public static DiscordColor Loading => DiscordColor.Orange;
    public static DiscordColor Info => DiscordColor.Aquamarine;
    public static DiscordColor Warning => DiscordColor.Orange;
    public static DiscordColor Important => DiscordColor.Orange;
    public static DiscordColor Processing => new("3d437e");
    public static DiscordColor AwaitingInput => DiscordColor.Orange;
    public static DiscordColor Success => new("77b255");
    public static DiscordColor HiddenSidebar => new("2f3136");
}
