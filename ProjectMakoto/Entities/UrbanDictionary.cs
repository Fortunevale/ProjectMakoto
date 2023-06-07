// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Entities;

internal sealed class UrbanDictionary
{
    public List[] list { get; set; }

    public sealed class List
    {
        public string definition { get; set; }
        public string permalink { get; set; }
        public int thumbs_up { get; set; }
        public object[] sound_urls { get; set; }
        public string author { get; set; }
        public string word { get; set; }
        public int defid { get; set; }
        public string current_vote { get; set; }
        public DateTime written_on { get; set; }
        public string example { get; set; }
        public int thumbs_down { get; set; }

        /// <summary>
        /// Return Thumbs Up/Thumbs Down Ratio.
        /// </summary>
        [JsonIgnore]
        public int RatingRatio
        {
            get
            {
                return this.thumbs_up - this.thumbs_down;
            }
        }
    }

}
