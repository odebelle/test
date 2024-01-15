namespace Dispatcher;

public class MappedObjectNotAssignedException : Exception
{
    public MappedObjectNotAssignedException() : base("Mapped object is null or not correctly assigned.")
    {
    }

    public MappedObjectNotAssignedException(string message) : base(message)
    {
    }
}