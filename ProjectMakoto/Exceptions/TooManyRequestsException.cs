namespace ProjectMakoto.Exceptions;
internal class TooManyRequestsException : Exception
{
    public TooManyRequestsException(string message = "") : base(message)
    {
    }
}
