using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RabbitMq;

public class RmqDeadLetterContext : DeadLetterContext
{
    private readonly IOptions<BrokerConfiguration> _brokerConfigurationOptions;

    public RmqDeadLetterContext(IOptions<BrokerConfiguration> brokerConfigurationOptions, IConfiguration configuration,ILogger logger) : base(configuration, logger)
    {
        _brokerConfigurationOptions = brokerConfigurationOptions;
    }

    public sealed override IDispatchDeadLetter CreateDeadLetterConsumer()
    {
        return new RmqDeadLetterConsumer(_brokerConfigurationOptions, Configuration!, Logger);
    }
}