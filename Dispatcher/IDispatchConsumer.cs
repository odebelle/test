using Shared.Models;

namespace Dispatcher;

public interface IDispatchConsumer
{
    ConsumerStatus GetConsumerStatus();
    Task<bool> StartConsumeAsync(CancellationToken stoppingToken);
    Task<bool> StopConsumeAsync(CancellationToken stoppingToken);
    ConsumerElement GetConsumerElement();


    /// <summary>
    /// Dismiss message and abort further operations
    /// </summary>
    /// <param name="reason"></param>
    /// <example>Ack("Object has no expectation to be processed")</example>
    void Ack(string reason);

    /// <summary>
    /// Dismiss message, abort further operations and throw error to send result to dead letter.
    /// </summary>
    /// <param name="reason">The reason of nack.</param>
    /// <param name="innerException"></param>
    /// <remarks>Use inside catch.</remarks>
    /// <example>
    /// <code>
    /// catch(Exception ex)
    /// {
    ///     // ... your code here
    ///     Consumer.Nack(ex.Message, ex)
    /// }
    /// </code>
    /// </example>
    void Nack(string reason, Exception? innerException = null);
}

public interface IDispatchConsumer<TSource, TPayoff> : IDispatchConsumer where TSource : class
{
    Task<bool> ToDeadLetterAsync(MessageHolder<TSource, TPayoff> messageHolder, params object[] args);

    event EventHandler<MapperEventArgs<TSource, TPayoff>>? Map;
    event EventHandler<MapperEventArgs<TSource, TPayoff>>? OperateMappedObject;
    event EventHandler<DeadLetterEventArgs>? OnDeadLetterError;
    void Stack(IMessageHolder<TSource, TPayoff> marshal);
}