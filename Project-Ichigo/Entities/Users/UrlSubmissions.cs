namespace Project_Ichigo.Entities;

internal class UrlSubmissions
{
    private bool _AcceptedTOS { get; set; } = false;
    public bool AcceptedTOS { get => _AcceptedTOS; set { _AcceptedTOS = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    private DateTime _LastTime { get; set; } = DateTime.MinValue;
    public DateTime LastTime { get => _LastTime; set { _LastTime = value; _ = Bot.DatabaseClient.SyncDatabase(); } }


    public List<string> AcceptedSubmissions { get; set; } = new();
}