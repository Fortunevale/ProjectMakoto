namespace ProjectIchigo.Entities;

internal class UserUpload
{
    public bool InteractionHandled { get; set; } = false;
    public DateTime TimeOut { get; set; } = DateTime.Now;
    public Stream UploadedData { get; set; }
    public int FileSize { get; set; } = 0;
}
