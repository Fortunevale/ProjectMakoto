namespace ProjectIchigo.Entities;

internal class UrbanDictionary
{
    public List[] list { get; set; }

    public class List
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
                return thumbs_up - thumbs_down;
            }
        }
    }

}
