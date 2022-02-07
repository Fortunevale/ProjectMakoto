namespace Project_Ichigo.Objects;

internal class Users
{
    public Dictionary<ulong, Info> List { get; set; } = new();

    internal class Info
    {
        public UrlSubmissions UrlSubmissions { get; set; } = new();
    }

    internal class UrlSubmissions
    {
        private bool _AcceptedTOS { get; set; } = false;
        public bool AcceptedTOS { get => _AcceptedTOS; set { _AcceptedTOS = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        private DateTime _LastTime { get; set; } = DateTime.MinValue;
        public DateTime LastTime { get => _LastTime; set { _LastTime = value; _ = Bot._databaseHelper.SyncDatabase(); } }


        public List<string> AcceptedSubmissions { get; set; } = new();
    }
}
