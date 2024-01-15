using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Dispatcher;

public sealed class MqttProducerService : MqttWorkerService, IMqttWorkerService
{
    public MqttProducerService(ILogger<MqttWorkerService> logger, IConfiguration configuration,
        IProducerElement workerElements,
        IProducerProcessManager processManager) : base()
    {
        Logger = logger;
        MqttBroker = configuration.GetValue<string>("MqttBroker");
        WorkerElements = new WorkerElements() { ProducerElement = workerElements };
        Route = WorkerElements.Route;
        Topics = new[]
        {
            $"{Route}_{WorkerControllerPath.Producer}_{WorkerAction.Pause}",
            $"{Route}_{WorkerControllerPath.Producer}_{WorkerAction.Resume}",
            $"{Route}_{WorkerControllerPath.Workers}"
        }.ToList();

        ProducerProcessManager = processManager;
    }
}