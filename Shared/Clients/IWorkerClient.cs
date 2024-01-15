using Shared.Models;

namespace Shared.Clients;

public interface IWorkerClient
{
    Task<bool?> ChangeProducerStateAsync(string route, string action, string? jobKey = null);
    Task<bool?> ChangeConsumerStateAsync(string route, string action, string? key = null);

    Task<WorkerElements?> GetWorkerElementsAsync(string route);
    Task<ProducerElement[]?> GetProducerElementsAsync(string route);
    Task<HttpResponseMessage> TestAsync(string route);
}