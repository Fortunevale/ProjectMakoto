namespace ProjectMakoto.Entities;

internal class DiscordWidget
{
    public string id { get; set; }
    public string name { get; set; }
    public string instant_invite { get; set; }
    public object[] channels { get; set; }
    public Member[] members { get; set; }
    public int presence_count { get; set; }

    public class Member
    {
        public string id { get; set; }
        public string username { get; set; }
        public string discriminator { get; set; }
        public object avatar { get; set; }
        public string status { get; set; }
        public string avatar_url { get; set; }
        public Game game { get; set; }
    }

    public class Game
    {
        public string name { get; set; }
    }

}
