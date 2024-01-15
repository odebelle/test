using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public interface IProducer<TSource> : IDisposable where TSource : class
{
    Task<TransitStatus> SendAsync(IEnumerable<TSource> items);
    Task<TransitStatus> StackMessage<TMarshal>(IMessageHolder<TSource, TMarshal> messageHolder);
    string Topic { get; }
}