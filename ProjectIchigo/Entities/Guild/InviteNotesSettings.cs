namespace ProjectIchigo.Entities
{
    public class InviteNotesSettings
    {
        public InviteNotesSettings(Guild guild)
        {
            Parent = guild;
        }

        private Guild Parent { get; set; }


        private Dictionary<string, InviteNotesDetails> _Notes { get; set; } = new();
        public Dictionary<string, InviteNotesDetails> Notes
        {
            get => _Notes;
            set
            {
                _Notes = value;
                _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "invitenotes", JsonConvert.SerializeObject(value), Bot.DatabaseClient.mainDatabaseConnection);
            }
        }
    }
}
