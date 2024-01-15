using System.Text.Json;
using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Enums;
using Shared.Models;

namespace RabbitMq;

public class RmqDispatchConsumer<TSource, TPayoff> : DispatchConsumer<TSource, TPayoff>
    where TSource : class where TPayoff : new()
{
    private IModel? _channel;
    private IConnection? _connection;
    private ConnectionFactory? _connectionFactory;
    private EventingBasicConsumer? _eventingBasicConsumer;
    private CancellationToken _stoppingToken;
    private RmqMarshalProducer<TSource>? _marshal;
    private readonly RmqDeadLetterProducer _deadLetterProducer;
    private string? _marshalName;

    private readonly string _queueName;
    private readonly BrokerConfiguration _brokerConfiguration;
    private readonly QueueDeclareOk _declaredQueue;
    private readonly string _deadLetterQueueName;
    private MessageHolder<TSource, TPayoff>? _messageHolder;
    private readonly string _exchangeName;
    private IConnection Connection => _connection ??= ConnectionFactory.CreateConnection(_brokerConfiguration.Endpoints);

    private ConnectionFactory ConnectionFactory =>
        _connectionFactory ??= new ConnectionFactory().SetBasicConnectionProperties(_brokerConfiguration);

    internal RmqDispatchConsumer(IOptions<BrokerConfiguration> brokerConfigurationOptions, IConfiguration configuration, IWorkerElements workerElements,
        ILogger logger) :
        base(configuration, workerElements, logger)
    {
        _brokerConfiguration = brokerConfigurationOptions.Value;
        _queueName = ConsumerElements.TopicName;
        _exchangeName = $"{ExchangePrefix.Default}.{typeof(TSource).GetTopicName()}";
        _deadLetterQueueName = $"{QueuePrefix.DeadLetter}.{_queueName}";
        _declaredQueue = SetDeclaredQueue(_brokerConfiguration.DefaultConsumerArguments);

        // Declare exchange 
        Channel.BasicQos(0, 10, true);
        Channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, true, false, null);
        Channel.QueueBind(_declaredQueue.QueueName, _exchangeName, "", null);

        _deadLetterProducer = new RmqDeadLetterProducer(_brokerConfiguration);
        _deadLetterProducer.OnBasicReturn += DeadLetterError!;
    }

    protected void SetMarshal()
    {
        if (string.IsNullOrEmpty(ConsumerElements.Marshal))
            throw new ArgumentException("Marshal name is missing.", nameof(ConsumerElements.Marshal));

        _marshalName = ConsumerElements.Marshal;
        _marshal = new RmqMarshalProducer<TSource>(Configuration,  _marshalName, Logger);
        _marshal.OnBasicReturn += MarshalError;
    }

    private void MarshalError(object? sender, BasicReturnEventArgs e)
    {
        _messageHolder!.ErrorUtc = DateTime.UtcNow;
        _messageHolder.Error = new MessageHolderError(new Exception($"Marshal return an error: '{e.ReplyText}'"));
        _messageHolder.Subject = $"{QueuePrefix.Marshal}.{_marshalName}";
        _messageHolder.VirtualHost = _brokerConfiguration.VirtualHost;

        e.Body = _messageHolder.SerializeToUtf8Bytes();

        DeadLetterError(sender!, new DeadLetterEventArgs(_messageHolder));
    }

    private void EventingBasicConsumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        _messageHolder = null;

        try
        {
            _messageHolder = Deserialize(System.Text.Encoding.UTF8.GetString(e.Body.ToArray()));
            if (_messageHolder.HasError)
            {
                _messageHolder.TransitStatus = TransitStatus.Unprocessed;
                throw new Exception(_messageHolder.Error!.Message);
            }

            ExecuteMapping(_messageHolder);

            if (_messageHolder.HasError)
                throw new Exception(_messageHolder.Error!.Message);

            ExecutePostMappingOperation(_messageHolder);

            if (_messageHolder.HasError)
            {
                _messageHolder.TransitStatus = TransitStatus.Unprocessed;
                throw new Exception(_messageHolder.Error!.Message);
            }

            Channel.BasicAck(e.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            if (_messageHolder != null)
            {
                _messageHolder.Error = new MessageHolderError(ex);

                _messageHolder.Topic = _exchangeName;
                _messageHolder.Subject = _queueName;
            }

            if (ToDeadLetterAsync(_messageHolder!, e, ex).Result)
                throw new Exception(_messageHolder!.Error!.Message);

            Channel.BasicNack(e.DeliveryTag, false, false);
        }
    }

    public override void Stack(IMessageHolder<TSource, TPayoff> marshal)
    {
        if (_marshalName is null)
            throw new NullReferenceException($"{nameof(WorkerElements.Marshal)} is null or not defined.");

        if (_messageHolder is not null)
            _marshal!.StackMessage(_messageHolder);
        else
            throw new NullReferenceException(nameof(_messageHolder));
    }

    public override ConsumerStatus GetConsumerStatus()
    {
        var result = Channel.IsClosed switch
        {
            false when EventingBasicConsumer.IsRunning => ConsumerStatus.Running,
            false => ConsumerStatus.Paused,
            _ => ConsumerStatus.Stopped
        };

        return result;
    }

    public override Task<bool> StartConsumeAsync(CancellationToken stoppingToken)
    {
        var result = true;
        try
        {
            if (!EventingBasicConsumer.IsRunning)
            {
                Channel.BasicConsume(_declaredQueue.QueueName, false, EventingBasicConsumer);
                _stoppingToken = stoppingToken;
                Task.Run(WaitForStopListeningAsync, CancellationToken.None);
            }
        }
        catch (Exception)
        {
            result = false;
        }

        return Task.FromResult(result);
    }

    private async Task? WaitForStopListeningAsync()
    {
        try
        {
            await Task.Delay(-1, _stoppingToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task? WaitForListenAsync()
    {
        try
        {
            await Task.Delay(-1, _stoppingToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public override Task<bool> StopConsumeAsync(CancellationToken stoppingToken)
    {
        var result = true;
        try
        {
            Channel.BasicCancel(EventingBasicConsumer.ConsumerTags[0] ?? string.Empty);
            _stoppingToken = stoppingToken;
            Task.Run(WaitForListenAsync, CancellationToken.None);
        }
        catch (Exception)
        {
            result = false;
        }

        return Task.FromResult(result);
    }

    private static MessageHolder<TSource, TPayoff> Deserialize(string payloadString)
    {
        MessageHolder<TSource, TPayoff>? messageHolder;
        try
        {
            messageHolder = JsonSerializer.Deserialize<MessageHolder<TSource, TPayoff>>(payloadString);
            if (messageHolder?.Source == null || !messageHolder.Source.Any())
            {
                throw new Exception("Payload is null or empty");
            }
        }
        catch (JsonException jsonException)
        {
            messageHolder = new MessageHolder<TSource, TPayoff>()
                { Subject = payloadString, Error = new MessageHolderError(jsonException) };
        }
        catch (Exception ex)
        {
            messageHolder = new MessageHolder<TSource, TPayoff>()
                { Subject = payloadString, Error = new MessageHolderError(ex) };
        }

        return messageHolder;
    }

    public override async Task<bool> ToDeadLetterAsync(MessageHolder<TSource, TPayoff>? messageHolder, params object[] args)
    {
        if (messageHolder == null)
            return false;

        messageHolder.ErrorUtc = DateTime.UtcNow;

        var result = await _deadLetterProducer.SendAsync(messageHolder);

        if (result == TransitStatus.Confirmed && args[0] is BasicDeliverEventArgs e)
            Channel.BasicAck(e.DeliveryTag, false);
        else
            result = TransitStatus.Unprocessed;

        return result == TransitStatus.Confirmed;
    }

    private IModel Channel
    {
        get
        {
            if (_channel != null) return _channel;

            _channel = Connection.CreateModel();
            _channel.ContinuationTimeout = new TimeSpan(0, 0, 60);

            return _channel;
        }
    }

    private EventingBasicConsumer EventingBasicConsumer
    {
        get
        {
            if (_eventingBasicConsumer is not null)
                return _eventingBasicConsumer;

            _eventingBasicConsumer = new EventingBasicConsumer(Channel);
            _eventingBasicConsumer.Received += EventingBasicConsumer_Received;
            _eventingBasicConsumer.Unregistered += OnRegisterAction;
            _eventingBasicConsumer.Registered += OnRegisterAction;

            return _eventingBasicConsumer;
        }
    }

    private void OnRegisterAction(object? sender, ConsumerEventArgs e)
    {
        SetConsumerElementAsync();
    }

    private QueueDeclareOk SetDeclaredQueue(IDictionary<string, object>? arguments = null)
    {
        Channel.ExchangeDeclare(DefaultExchangeName.DeadLetter, ExchangeType.Topic, true);
        // arguments ??= new Dictionary<string, object>();
        //
        // arguments.TryAdd("x-dead-letter-exchange", DefaultExchangeName.DeadLetter);
        // arguments.TryAdd("x-dead-letter-routing-key", _deadLetterQueueName);

        var dlq = Channel.QueueDeclare(_deadLetterQueueName, true, false, false, arguments);
        Channel.QueueBind(dlq.QueueName, DefaultExchangeName.DeadLetter, dlq.QueueName);

        return Channel.QueueDeclare(_queueName, true, false, false, arguments);
    }
}