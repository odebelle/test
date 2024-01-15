using Shared.Models;

namespace Dispatcher;

public class MapperEventArgs<TSource, TPayoff> : EventArgs
{
    public IMessageHolder<TSource, TPayoff>? MessageHolder { get; }

    public MapperEventArgs(IMessageHolder<TSource, TPayoff>? messageHolder)
    {
        MessageHolder = messageHolder;
    }
}