namespace Kaffeemaschine.Exceptions;

internal class MemberNotFoundException : Exception
{
    public MemberNotFoundException(string message) : base(message)
    {
    }
}
