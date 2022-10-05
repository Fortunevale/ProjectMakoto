namespace ProjectIchigo.Exceptions;
internal class TimedOutException : Exception
{
    public TimedOutException(string message = "") : base(message)
    {
    }
}
