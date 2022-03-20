namespace Project_Ichigo.Objects;
internal class AfkStatus
{
    private string _Reason { get; set; } = "";
    public string Reason { get => _Reason; set { _Reason = value; _ = Bot._databaseClient.SyncDatabase(); } }



    private DateTime _TimeStamp { get; set; } = DateTime.UnixEpoch;
    public DateTime TimeStamp { get => _TimeStamp; set { _TimeStamp = value; _ = Bot._databaseClient.SyncDatabase(); } }


    private long _MessagesAmount { get; set; } = 0;
    public long MessagesAmount { get => _MessagesAmount; set { _MessagesAmount = value; _ = Bot._databaseClient.SyncDatabase(); } }



    private List<AfkStatusMessageCache> _Messages { get; set; } = new();
    public List<AfkStatusMessageCache> Messages
    {
        get
        {
            if (_Messages == null)
                _Messages = new();

            return _Messages;
        }

        set
        {
            _Messages = value;
        }
    }



    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
