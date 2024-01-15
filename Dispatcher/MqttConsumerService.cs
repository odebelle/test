using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Dispatcher;

public sealed class MqttConsumerService : MqttWorkerService, IMqttWorkerService
{
    public MqttConsumerService(ILogger<MqttWorkerService> logger, IConfiguration configuration, IWorkerElements workerElements,
        IProcessManager processManager) : base()
    {
        Logger = logger;
        MqttBroker = configuration.GetValue<string>("MqttBroker");
        WorkerElements = workerElements;
        Route = WorkerElements.Route;
        Topics = new[]
        {
            $"{Route}_{WorkerControllerPath.Consumer}_{WorkerAction.Pause}",
            $"{Route}_{WorkerControllerPath.Consumer}_{WorkerAction.Resume}",
            $"{Route}_{WorkerControllerPath.Workers}"
        }.ToList();

        ConsumerProcessManager = processManager as IConsumerProcessManager;
    }
}