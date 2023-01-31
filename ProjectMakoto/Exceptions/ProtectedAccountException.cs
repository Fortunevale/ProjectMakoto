namespace ProjectMakoto.Exceptions;

internal class ProtectedAccountException : Exception
{
    public ProtectedAccountException(string message) : base(message)
    {
    }
}
