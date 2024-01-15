using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class DeadLetterContext:IDispatcherContext
{
    public ILogger Logger { get; }
    public IConfiguration? Configuration { get; }
    public DispatcherConfiguration? DispatcherConfiguration { get; set; }

    protected DeadLetterContext(IConfiguration configuration, ILogger logger)
    {
        Logger = logger;
        Configuration = configuration;
    }
    public IDispatchConsumer<TSource, TMapped> CreateSimpleConsumer<TSource, TMapped>() where TSource : class where TMapped : new()
    {
        throw new NotImplementedException();
    }

    public virtual IDispatchDeadLetter CreateDeadLetterConsumer()
    {
        throw new NotImplementedException();
    }

    public IProducer<TSource> CreateProducer<TSource>() where TSource : class
    {
        throw new NotImplementedException();
    }

    public Task<TransitStatus> PublishAsync<TSource>(IEnumerable<TSource> message) where TSource : class
    {
        throw new NotImplementedException();
    }

    public IDispatchConsumer<object, object> CreateSortedQueueConsumer()
    {
        throw new NotImplementedException();
    }

    public IDispatchConsumer<TSource, TMapped> CreateStacker<TSource, TMapped>() where TSource : class where TMapped : new()
    {
        throw new NotImplementedException();
    }
    
    
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