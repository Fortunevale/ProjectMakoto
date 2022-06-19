namespace ProjectIchigo.Entities;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class PreventCommandDeletionAttribute : Attribute
{
    public readonly bool PreventDeleteCommandMessage;

    public PreventCommandDeletionAttribute(bool PreventDeleteMessage = true)
    {
        this.PreventDeleteCommandMessage = PreventDeleteMessage;
    }
}