namespace Project_Ichigo.Objects;
internal class AfkStatus
{
    private string _Reason { get; set; } = "";
    public string Reason { get => _Reason; set { _Reason = value; _ = Bot._databaseHelper.SyncDatabase(); } }
    
    private DateTime _TimeStamp { get; set; } = DateTime.MinValue;
    public DateTime TimeStamp { get => _TimeStamp; set { _TimeStamp = value; _ = Bot._databaseHelper.SyncDatabase(); } }
}
