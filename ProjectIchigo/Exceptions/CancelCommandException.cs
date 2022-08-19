namespace ProjectIchigo.Exceptions;
internal class CancelCommandException : Exception
{
    public CancelCommandException(string message, CommandContext context) : base(message)
    {
        this.context = context;
    }

    public CommandContext context { get; set; }
}
