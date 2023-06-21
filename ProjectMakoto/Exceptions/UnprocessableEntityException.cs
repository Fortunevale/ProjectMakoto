namespace ProjectMakoto.Exceptions;

public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message) : base(message)
    {
    }
}
