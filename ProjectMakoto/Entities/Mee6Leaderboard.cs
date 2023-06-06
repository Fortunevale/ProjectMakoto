// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class Mee6Leaderboard
{
    public bool admin { get; set; }
    public string banner_url { get; set; }
    public string country { get; set; }
    public Guild guild { get; set; }
    public bool is_member { get; set; }
    public int page { get; set; }
    public object player { get; set; }
    public Player[] players { get; set; }
    public object[] role_rewards { get; set; }
    public object user_guild_settings { get; set; }
    public int[] xp_per_message { get; set; }
    public float xp_rate { get; set; }

    public sealed class Guild
    {
        public bool allow_join { get; set; }
        public string icon { get; set; }
        public string id { get; set; }
        public bool invite_leaderboard { get; set; }
        public string leaderboard_url { get; set; }
        public string name { get; set; }
        public bool premium { get; set; }
    }

    public sealed class Player
    {
        public string avatar { get; set; }
        public int[] detailed_xp { get; set; }
        public string discriminator { get; set; }
        public string guild_id { get; set; }
        public string id { get; set; }
        public int level { get; set; }
        public int message_count { get; set; }
        public string username { get; set; }
        public int xp { get; set; }
    }

}
