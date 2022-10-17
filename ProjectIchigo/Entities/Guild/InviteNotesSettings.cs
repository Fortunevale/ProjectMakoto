namespace ProjectIchigo.Entities;

public class InviteNotesSettings
{
    public InviteNotesSettings(Guild guild)
    {
        Parent = guild;
    }

    private Guild Parent { get; set; }


    public Dictionary<string, InviteNotesDetails> Notes { get; set; } = new();
}
