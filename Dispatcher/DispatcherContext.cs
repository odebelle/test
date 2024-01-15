using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class DispatcherContext : IDispatcherContext
{
    protected DispatcherContext(IConfiguration configuration, IWorkerElements workerElements, ILogger<DispatcherContext> logger)
    {
        Configuration = configuration;
        WorkerElements = workerElements;
    }

    public virtual IDispatchConsumer<TSource, TMapped> CreateSimpleConsumer<TSource, TMapped>()
        where TSource : class where TMapped : new()
    {
        throw new NotImplementedException();
    }

    public virtual IDispatchConsumer<TSource, TMapped> CreateStacker<TSource, TMapped>() where TSource : class where TMapped : new()
    {
        throw new NotImplementedException();
    }
    public virtual IDispatchDeadLetter CreateDeadLetterConsumer()
    {
        throw new NotImplementedException();
    }

    public virtual IProducer<TSource> CreateProducer<TSource>() where TSource : class
    {
        throw new NotImplementedException();
    }

    public virtual Task<TransitStatus> PublishAsync<TSource>(IEnumerable<TSource> message) where TSource : class
    {
        using var producer = CreateProducer<TSource>();
        return producer.SendAsync(message);
    }

    public DispatcherConfiguration? DispatcherConfiguration { get; set; }
    public IConfiguration? Configuration { get; }
    public virtual IDispatchConsumer<object, object> CreateSortedQueueConsumer()
    {
        throw new NotImplementedException();
    }

    public IWorkerElements WorkerElements { get; init; }

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