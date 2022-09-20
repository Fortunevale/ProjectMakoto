namespace ProjectIchigo.Exceptions;
internal class TooManyRequestsException : Exception
{
    public TooManyRequestsException(string message = "") : base(message)
    {
    }
}
