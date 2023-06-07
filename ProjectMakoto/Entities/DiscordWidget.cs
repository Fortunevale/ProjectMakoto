// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class DiscordWidget
{
    public string id { get; set; }
    public string name { get; set; }
    public string instant_invite { get; set; }
    public object[] channels { get; set; }
    public Member[] members { get; set; }
    public int presence_count { get; set; }

    public sealed class Member
    {
        public string id { get; set; }
        public string username { get; set; }
        public string discriminator { get; set; }
        public object avatar { get; set; }
        public string status { get; set; }
        public string avatar_url { get; set; }
        public Game game { get; set; }
    }

    public sealed class Game
    {
        public string name { get; set; }
    }

}
