namespace ProjectIchigo.Exceptions;
internal class InternalServerErrorException : Exception
{
    public InternalServerErrorException(string message = "") : base(message)
    {
    }
}
