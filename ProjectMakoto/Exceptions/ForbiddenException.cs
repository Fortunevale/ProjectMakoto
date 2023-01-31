namespace ProjectMakoto.Exceptions;
internal class ForbiddenException : Exception
{
    public ForbiddenException(string message = "") : base(message)
    {
    }
}
