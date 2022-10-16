namespace ProjectIchigo.Events
{
    internal class InviteNoteEvents
    {
        internal InviteNoteEvents(Bot _bot)
        {
            this._bot = _bot;
        }

        public Bot _bot { private get; set; }
    }
}
