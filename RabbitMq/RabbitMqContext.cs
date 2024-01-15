using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public class RabbitProducer(
    IConfiguration configuration,
    IProducerElement element,
    ILogger<DispatcherContext> logger)
    : IDispatcherProducer
{
    public IDispatchDeadLetter CreateDeadLetterConsumer()
    {
        throw new NotImplementedException();
    }

    public IProducer<TSource> CreateProducer<TSource>() where TSource : class
    {
        element!.EnsureIsCorrectlySet();
        return new RmqProducer<TSource>(configuration, logger);
    }

    public async Task<TransitStatus> PublishAsync<TSource>(IEnumerable<TSource> message) where TSource : class
    {
        using var producer = CreateProducer<TSource>();
        var result = await producer.SendAsync(message);
        return result;
    }

    public DispatcherConfiguration? DispatcherConfiguration { get; set; }
    public IConfiguration? Configuration => configuration;
    
    
    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        // Dispose of unmanaged resources.
        Dispose(true);
        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // TODO: dispose managed state (managed objects).
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        _disposed = true;
    }

    #endregion
}

public class RabbitMqContext : DispatcherContext
{
    private readonly IOptions<BrokerConfiguration> _brokerConfigurationOptions;
    private readonly IConfiguration _configuration;
    private readonly IWorkerElements _workerElements;
    private readonly ILogger<RabbitMqContext> _logger;

    public RabbitMqContext(IOptions<BrokerConfiguration> brokerConfigurationOptions, IConfiguration configuration, IWorkerElements workerElements, ILogger<RabbitMqContext> logger) : 
        base(configuration, workerElements, logger)
    {
        _brokerConfigurationOptions = brokerConfigurationOptions;
        _configuration = configuration;
        _workerElements = workerElements;
        _logger = logger;
    }

    public sealed override IProducer<TSource> CreateProducer<TSource>() where TSource : class
    {
        WorkerElements.ProducerElement!.EnsureIsCorrectlySet();
        return new RmqProducer<TSource>(_configuration, _logger);
    }

    public sealed override IDispatchConsumer<TSource, TMapped> CreateSimpleConsumer<TSource, TMapped>()
    {
        WorkerElements.ConsumerElement!.EnsureIsCorrectlySet();

        var result = new RmqDispatchConsumer<TSource, TMapped>(_brokerConfigurationOptions, _configuration, _workerElements, _logger);

        return result;
    }

    public sealed override IDispatchConsumer<TSource, TMapped> CreateStacker<TSource, TMapped>()
    {
        WorkerElements.ConsumerElement!.EnsureIsCorrectlySet();

        var result = new RmqStacker<TSource, TMapped>(_brokerConfigurationOptions,_configuration, _workerElements, _logger);

        return result;
    }

    public override IDispatchConsumer<object, object> CreateSortedQueueConsumer()
    {
        WorkerElements.ConsumerElement!.EnsureIsCorrectlySet();

        var result = new RmqSortedQueueConsumer(_brokerConfigurationOptions, _configuration, _workerElements, _logger);

        return result;
    }

    public sealed override IDispatchDeadLetter CreateDeadLetterConsumer()
    {
        WorkerElements.ConsumerElement!.EnsureIsCorrectlySet();
        WorkerElements.DeadLetterElement = new ConsumerElement(WorkerElements.ConsumerElement, WorkerElements.Route);

        return new RmqDeadLetterConsumer(_brokerConfigurationOptions, _configuration, _logger);
    }
}