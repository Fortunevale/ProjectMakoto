namespace ProjectMakoto.Exceptions;
internal class InternalServerErrorException : Exception
{
    public InternalServerErrorException(string message = "") : base(message)
    {
    }
}
