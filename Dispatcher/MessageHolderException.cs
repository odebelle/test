namespace Dispatcher;

public class MessageHolderException : Exception
{
    public MessageHolderException()
    {
    }

    public MessageHolderException(string message) : base(message)
    {
    }

    public MessageHolderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}