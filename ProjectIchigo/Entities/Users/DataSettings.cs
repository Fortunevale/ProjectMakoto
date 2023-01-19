namespace ProjectIchigo.Entities;
public class DataSettings
{
    public DateTime LastDataRequest { get; set; } = DateTime.MinValue;

    public bool DeletionRequested { get; set; } = false;
    public DateTime DeletionRequestDate { get; set; } = DateTime.MinValue;
}
