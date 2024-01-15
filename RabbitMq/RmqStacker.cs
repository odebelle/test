using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace RabbitMq;

public class RmqStacker<TSource, TPayoff> : RmqDispatchConsumer<TSource, TPayoff>
    where TSource : class where TPayoff : new()
{
    internal RmqStacker(IOptions<BrokerConfiguration> brokerConfigurationOptions, IConfiguration configuration, IWorkerElements workerElements,
        ILogger logger) : base(brokerConfigurationOptions, configuration, workerElements, logger)
    {
        SetMarshal();
    }
}