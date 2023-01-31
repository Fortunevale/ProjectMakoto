namespace ProjectMakoto.Entities;

public class InteractionResult<T>
{
    public InteractionResult(T result)
    {
        Result = result;
    }
    
    public InteractionResult(Exception exception)
    {
        Exception = exception;
    }

    public T Result { get; set; }

    public bool Failed { get { return TimedOut || Cancelled; } }

    public bool TimedOut { get { return (Exception is not null && Exception.GetType() == typeof(TimedOutException)); } }

    public bool Cancelled { get { return (Exception is not null && Exception.GetType() == typeof(CancelException)); } }

    public bool Errored { get { return Exception is not null; } }

    public Exception Exception { get; set; }
}
