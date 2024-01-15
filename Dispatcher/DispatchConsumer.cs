using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models;

namespace Dispatcher;

public abstract class DispatchConsumer<TSource, TPayoff> : IDispatchConsumer<TSource, TPayoff>
    where TSource : class where TPayoff : new()
{
    // public IMqttServiceForProcesses MqttServiceForProcesses { get; }
    private string? _lastResult;
    private AckStatus _ackStatus = AckStatus.None;
    private string? _currentAcknowledgmentReason;

    protected ConsumerElement ConsumerElements { get; init; }

    public IConfiguration Configuration { get; }

    public IWorkerElements Elements { get; }
    public ILogger Logger { get; }

    public event EventHandler<MapperEventArgs<TSource, TPayoff>>? Map;
    public event EventHandler<MapperEventArgs<TSource, TPayoff>>? OperateMappedObject;
    public event EventHandler<DeadLetterEventArgs>? OnDeadLetterError;
    public abstract void Stack(IMessageHolder<TSource, TPayoff> marshal);

    // TODO : IS Needed anymore?   
    // MqttServiceForProcesses = serviceProvider.GetRequiredService<IMqttServiceForProcesses>();

    protected DispatchConsumer(IConfiguration configuration, IWorkerElements workerElements, ILogger logger)
    {
        Configuration = configuration;
        Elements = workerElements;
        Logger = logger;

        ConsumerElements = workerElements.ConsumerElement ?? throw new NullReferenceException("Consumer element is null");
        ConsumerElements.EnsureIsCorrectlySet();
    }


    protected void DeadLetterError(object sender, EventArgs e)
    {
        OnDeadLetterError?.Invoke(sender, (DeadLetterEventArgs)e);
    }

    public void Ack(string reason)
    {
        _currentAcknowledgmentReason = reason;
        _ackStatus = AckStatus.Ack;
    }

    public void Nack(string reason, Exception? innerException)
    {
        throw new Exception(_currentAcknowledgmentReason, innerException);
    }

    private static void EnsurePayloadIsAvailable(IMessageHolder<TSource> messageHolder)
    {
        if (messageHolder.Source is null || !messageHolder.Source.Any())
            throw new MessageHolderException("Payload is not completed.");
    }

    private static void EnsureMappedObjectIsSet(IMessageHolder<TSource, TPayoff> messageHolder)
    {
        if (messageHolder.Payoff is null)
            throw new MessageHolderException("Payload is not completed.");
    }

    protected void ExecuteMapping(MessageHolder<TSource, TPayoff> messageHolder)
    {
        try
        {
            EnsurePayloadIsAvailable(messageHolder);
            _currentAcknowledgmentReason = null;
            _ackStatus = AckStatus.None;
            Map?.Invoke(this, new MapperEventArgs<TSource, TPayoff>(messageHolder));
            switch (_ackStatus)
            {
                case AckStatus.Nack:
                case AckStatus.Ack:
                    break;
                case AckStatus.None:
                default:
                    messageHolder.EnsureMappingIsComplete();
                    break;
            }

            messageHolder.TransitStatus = TransitStatus.Accepted;
        }
        catch (Exception e)
        {
            messageHolder.Error = new MessageHolderError(e);
            messageHolder.TransitStatus = TransitStatus.Unprocessed;
        }

        _lastResult = messageHolder.GetResult();

        SetConsumerElementAsync(DateTime.Now);
    }

    /// <summary>
    /// Dispatch operation. It will processed only if Route is not set.
    /// </summary>
    /// <param name="messageHolder"></param>
    protected void ExecutePostMappingOperation(MessageHolder<TSource, TPayoff> messageHolder)
    {
        if (_ackStatus == AckStatus.None)
        {
            try
            {
                EnsureMappedObjectIsSet(messageHolder);

                OperateMappedObject?.Invoke(this, new MapperEventArgs<TSource, TPayoff>(messageHolder));

                messageHolder.TransitStatus = TransitStatus.Redirect;
            }
            catch (Exception e)
            {
                messageHolder.Error = new MessageHolderError(e);
                messageHolder.TransitStatus = TransitStatus.Unprocessed;
            }
        }

        _lastResult = messageHolder.GetResult();

        SetConsumerElementAsync();
    }

    protected void SetConsumerElementAsync(DateTime? lastSuccessful = null,
        CancellationToken? cancellationToken = null)
    {
        var now = DateTime.Now;

        lastSuccessful ??= ConsumerElements.LastSuccessfulConsumption;

        ConsumerElements.FirstConsumption ??= now;
        ConsumerElements.Information = _lastResult;
        ConsumerElements.LastConsumption = now;
        ConsumerElements.LastSuccessfulConsumption = lastSuccessful;
        ConsumerElements.ConsumerStatus = GetConsumerStatus();
    }

    public abstract ConsumerStatus GetConsumerStatus();
    public abstract Task<bool> StartConsumeAsync(CancellationToken stoppingToken);

    public abstract Task<bool> StopConsumeAsync(CancellationToken stoppingToken);

    public ConsumerElement GetConsumerElement()
    {
        ConsumerElements.ConsumerStatus = GetConsumerStatus();

        return ConsumerElements;
    }

    public abstract Task<bool> ToDeadLetterAsync(MessageHolder<TSource, TPayoff>? messageHolder, params object[] args);

    private enum AckStatus
    {
        Ack,
        Nack,
        None
    }
}