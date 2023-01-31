namespace ProjectMakoto.Entities;

public class ReminderItem
{
    public string UUID { get; set; } = Guid.NewGuid().ToString();

    public string CreationPlace { get; set; }

    private string _Description { get; set; }
    public string Description
    {
        get => _Description;
        set
        {
            if (value.Length > 512)
                throw new ArgumentException("The description cannot be longer than 512 characters.");

            _Description = value;
        }
    }

    public DateTime DueTime { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
