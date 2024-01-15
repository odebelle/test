using Shared.Models;

namespace ServerSideOperator.Services;

public interface IMqttService
{
    Task SendToTopic(string topic, string? payload = null);
    Task<List<string>> PingWorkersAsync();
    Task SetElementsAsync(WorkerElements elements);
    Task SetProducerAsync(IProducerElement element);
    Task SetConsumerAsync(IConsumerElement element);

    event Action? OnProducerChange;
    event Action? OnConsumerChange;
    event EventHandler<DaemonMessageEventArgs>? OnDaemonChange;

    List<IConsumerElement> Consumers { get; }
    List<IProducerElement> Producers { get; }
    Task ConnectAsync(CancellationToken? cancellationToken);
}