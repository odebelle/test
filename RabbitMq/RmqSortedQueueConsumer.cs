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

public sealed class RmqSortedQueueConsumer : DispatchConsumer<object, object>
{
    private IModel? _channel;
    private EventingBasicConsumer? _eventingBasicConsumer;
    private readonly string _deadLetterQueueName;
    private readonly string _queueName;
    private IConnection? _connection;
    private ConnectionFactory? _connectionFactory;
    private readonly BrokerConfiguration _brokerConfiguration;

    private CancellationToken _stoppingToken;

    // private readonly string _exchangeName;
    private readonly RmqDeadLetterProducer _deadLetterProducer;
    private IConnection Connection => _connection ??= ConnectionFactory.CreateConnection(_brokerConfiguration.Endpoints);

    private ConnectionFactory ConnectionFactory =>
        _connectionFactory ??= new ConnectionFactory().SetBasicConnectionProperties(_brokerConfiguration);

    public RmqSortedQueueConsumer(IOptions<BrokerConfiguration> brokerConfiguration, IConfiguration configuration, IWorkerElements elements, ILogger logger) : 
        base(configuration, elements, logger)
    {
        _brokerConfiguration = brokerConfiguration.Value;

        if (string.IsNullOrEmpty(elements.ConsumerElement?.TopicName ?? string.Empty))
            throw new Exception("Consumer Topic not defined.");
        var qn = elements.ConsumerElement!.TopicName;
        _queueName = $"{QueuePrefix.Marshal}.{qn}";
        var exchangeName = $"{ExchangePrefix.Marshal}.{qn}";
        _deadLetterQueueName = $"{QueuePrefix.DeadLetter}.{qn}";
        var declaredQueue = SetDeclaredQueue(_brokerConfiguration.DefaultConsumerArguments);

        // Declare exchange 
        Channel.BasicQos(0, 1, true);
        Channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, true, false, null);
        Channel.QueueBind(declaredQueue.QueueName, exchangeName, "", null);

        _deadLetterProducer = new RmqDeadLetterProducer(_brokerConfiguration);
        _deadLetterProducer.OnBasicReturn += DeadLetterError!;
    }

    /// <summary>
    /// This method is not intended to be used in this context. This wil throw an exception.
    /// </summary>
    /// <param name="marshal"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void Stack(IMessageHolder<object, object> marshal) => throw new NotImplementedException();

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
                Channel.BasicConsume(_queueName, false, EventingBasicConsumer);
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

    public override async Task<bool> ToDeadLetterAsync(MessageHolder<object, object>? messageHolder, params object[] args)
    {
        if (messageHolder is null)
        {
            return false;
        }

        messageHolder.Subject = _queueName;
        messageHolder.VirtualHost = _brokerConfiguration.VirtualHost;
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
        const string arg = "x-single-active-consumer";

        arguments ??= new Dictionary<string, object>();
        var dlqArgument = new Dictionary<string, object>(arguments);

        arguments[arg] = true;

        Channel.ExchangeDeclare(DefaultExchangeName.DeadLetter, ExchangeType.Topic, true);
        var dlq = Channel.QueueDeclare(_deadLetterQueueName, true, false, false, dlqArgument);
        Channel.QueueBind(dlq.QueueName, DefaultExchangeName.DeadLetter, dlq.QueueName);

        return Channel.QueueDeclare(_queueName, true, false, false, arguments);
    }

    private void EventingBasicConsumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        MessageHolder<object, object>? messageHolder = null;

        try
        {
            messageHolder = Deserialize(System.Text.Encoding.UTF8.GetString(e.Body.ToArray()));
            if (messageHolder.HasError)
            {
                messageHolder.TransitStatus = TransitStatus.Unprocessed;
                throw new Exception(messageHolder.Error!.Message);
            }

            ExecuteMapping(messageHolder);

            if (messageHolder.HasError)
                throw new Exception(messageHolder.Error!.Message);

            ExecutePostMappingOperation(messageHolder);

            if (messageHolder.HasError)
            {
                messageHolder.TransitStatus = TransitStatus.Unprocessed;
                throw new Exception(messageHolder.Error!.Message);
            }


            Channel.BasicAck(e.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            if (messageHolder != null)
                messageHolder.Error = new MessageHolderError(ex);

            if (ToDeadLetterAsync(messageHolder, e, ex).Result)
                throw new Exception(messageHolder!.Error!.Message);
        }
    }

    private static MessageHolder<object, object> Deserialize(string payloadString)
    {
        MessageHolder<object, object>? messageHolder;
        try
        {
            messageHolder = JsonSerializer.Deserialize<MessageHolder<object, object>>(payloadString);
            if (messageHolder?.Source == null || !messageHolder.Source.Any())
            {
                throw new Exception("Payload is null or empty");
            }
        }
        catch (JsonException jsonException)
        {
            messageHolder = new MessageHolder<object, object>()
                { Subject = payloadString, Error = new MessageHolderError(jsonException) };
        }
        catch (Exception ex)
        {
            messageHolder = new MessageHolder<object, object>()
                { Subject = payloadString, Error = new MessageHolderError(ex) };
        }

        return messageHolder;
    }
}