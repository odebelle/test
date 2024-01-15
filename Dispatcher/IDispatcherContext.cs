using Microsoft.Extensions.Configuration;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public interface IDispatcherContext : IDisposable
{
    IDispatchConsumer<TSource, TMapped> CreateSimpleConsumer<TSource, TMapped>()
        where TSource : class where TMapped : new();

    IDispatchDeadLetter CreateDeadLetterConsumer();
    IProducer<TSource> CreateProducer<TSource>() where TSource : class;
    Task<TransitStatus> PublishAsync<TSource>(IEnumerable<TSource> message) where TSource : class;
    DispatcherConfiguration? DispatcherConfiguration { get; set; }
    IConfiguration? Configuration { get; }
    IDispatchConsumer<object, object> CreateSortedQueueConsumer();
    IDispatchConsumer<TSource, TMapped> CreateStacker<TSource, TMapped>() where TSource : class where TMapped : new();
}

public interface IDispatcherProducer : IDisposable
{
    IDispatchDeadLetter CreateDeadLetterConsumer();
    IProducer<TSource> CreateProducer<TSource>() where TSource : class;
    Task<TransitStatus> PublishAsync<TSource>(IEnumerable<TSource> message) where TSource : class;
    DispatcherConfiguration? DispatcherConfiguration { get; set; }
    IConfiguration? Configuration { get; }
}