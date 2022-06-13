namespace ProjectIchigo.Exceptions;

internal class AlreadyAppliedException : Exception
{
    public AlreadyAppliedException(string message) : base(message)
    {
    }
}
