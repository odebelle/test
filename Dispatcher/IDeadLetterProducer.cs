using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public interface IDeadLetterProducer : IDisposable
{
    Task<TransitStatus> SendAsync(object items);
}