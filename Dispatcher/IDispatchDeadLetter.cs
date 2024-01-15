namespace Dispatcher;

public interface IDispatchDeadLetter : IDispatchConsumer
{
    event EventHandler<DeadLetterEventArgs>? Store;
}