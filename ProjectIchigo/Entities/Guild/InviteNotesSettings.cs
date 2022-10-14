namespace ProjectIchigo.Entities
{
    public class InviteNotesSettings
    {
        public InviteNotesSettings(Guild guild)
        {
            Parent = guild;
        }

        private Guild Parent { get; set; }



        private bool _InviteNotesEnabled { get; set; } = false;
        public bool InviteNotesEnabled
        {
            get => _InviteNotesEnabled;
            set
            {
                _InviteNotesEnabled = value;
                _ = Bot.DatabaseClient.UpdateValue("guilds", "serverid", Parent.ServerId, "invitenotes", value, Bot.DatabaseClient.mainDatabaseConnection);
            }
        }
    }
}
