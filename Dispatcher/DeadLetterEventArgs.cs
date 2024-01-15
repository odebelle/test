namespace Dispatcher;

public class DeadLetterEventArgs : EventArgs
{
    public object? MessageHolder { get; }

    public DeadLetterEventArgs(object? messageHolder)
    {
        MessageHolder = messageHolder;
    }
}