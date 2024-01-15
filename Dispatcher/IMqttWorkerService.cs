using Shared.Models;

namespace Dispatcher;

public interface IMqttWorkerService
{
    Task<bool?> SendConsumerElementAsync(ConsumerElement element, CancellationToken cancellationToken);
    Task<bool?> SendProducerElementAsync(IProducerElement element, CancellationToken cancellationToken);
    Task RegisterProducerAsync(string route, IProducerProcessManager processManager);
    Task InitializeAsync(CancellationToken? token = null);
}