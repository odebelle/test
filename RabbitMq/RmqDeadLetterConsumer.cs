using System.Text.Json;
using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace RabbitMq;

public class RmqDeadLetterConsumer : IDispatchDeadLetter
{
    private readonly ILogger _logger;
    private IModel? _channel;
    private IConnection? _connection;
    private ConnectionFactory? _connectionFactory;
    private EventingBasicConsumer? _eventingBasicConsumer;
    private CancellationToken _stoppingToken;

    private IConfiguration Configuration { get; }
    private string ExchangeName { get; }
    private string QueueName { get; }
    public BrokerConfiguration BrokerConfigurationOptions { get; }
    private QueueDeclareOk DeclaredQueue { get; }
    private IConnection Connection => _connection ??= ConnectionFactory.CreateConnection(BrokerConfigurationOptions.Endpoints);

    private ConnectionFactory ConnectionFactory =>
        _connectionFactory ??= new ConnectionFactory().SetBasicConnectionProperties(BrokerConfigurationOptions);

    public ConsumerElement GetConsumerElement() => throw new NotImplementedException();
    public void Ack(string reason)=>throw new NotImplementedException();
    public void Nack(string reason, Exception? innerException)=> throw new NotImplementedException();

    public IMqttWorkerService? MqttServiceForProcesses { get; set; } = null;

    // internal RmqDeadLetterConsumer(ConsumerElement consumerElement, BrokerConfiguration brokerConfiguration,
    //     IReverseProxyClient reverseProxyClient)
    internal RmqDeadLetterConsumer(IOptions<BrokerConfiguration> brokerConfigurationOptions, IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        Configuration = configuration;
        BrokerConfigurationOptions = brokerConfigurationOptions.Value;

        QueueName = DefaultExchangeName.DeadLetter;
        ExchangeName = DefaultExchangeName.DeadLetter;
        DeclaredQueue = SetDeclaredQueue(BrokerConfigurationOptions.DefaultConsumerArguments);

        // Declare exchange 
        Channel.BasicQos(0, 10, true);
        Channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, true, false, null);
        Channel.QueueBind(DeclaredQueue.QueueName, ExchangeName, QueueName, null);
    }

    private static MessageHolder Deserialize(string payloadString)
    {
        MessageHolder messageHolder;
        try
        {
            messageHolder = JsonSerializer.Deserialize<MessageHolder>(payloadString) ?? throw new InvalidOperationException();
        }
        catch (JsonException jsonException)
        {
            messageHolder = new MessageHolder()
                { Subject = payloadString, Error = new MessageHolderError(jsonException) };
        }
        catch (Exception ex)
        {
            messageHolder = new MessageHolder()
                { Subject = payloadString, Error = new MessageHolderError(ex) };
        }

        messageHolder.FixProperties();
        return messageHolder;
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

            return _eventingBasicConsumer;
        }
    }

    private void EventingBasicConsumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            var messageHolder = Deserialize(System.Text.Encoding.UTF8.GetString(e.Body.ToArray()));

            try
            {
                // use mapping to process dead letter and ignore next step 'ExecutePostMappingOperation'
                Store?.Invoke(this, new DeadLetterEventArgs(messageHolder));

                Channel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception)
            {
                Channel.BasicNack(e.DeliveryTag, false, true);
            }
        }
        catch (Exception)
        {
            Channel.BasicNack(e.DeliveryTag, false, true);
        }
    }

    private QueueDeclareOk SetDeclaredQueue(IDictionary<string, object>? arguments = null)
    {
        return Channel.QueueDeclare(QueueName, true, false, false, arguments);
    }

    public ConsumerStatus GetConsumerStatus()
    {
        var result = Channel.IsClosed switch
        {
            false when EventingBasicConsumer.IsRunning => ConsumerStatus.Running,
            false => ConsumerStatus.Paused,
            _ => ConsumerStatus.Stopped
        };

        return result;
    }

    public Task<bool> StartConsumeAsync(CancellationToken stoppingToken)
    {
        var result = true;
        try
        {
            if (!EventingBasicConsumer.IsRunning)
            {
                Channel.BasicConsume(DeclaredQueue.QueueName, false, EventingBasicConsumer);
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

    public Task<bool> StopConsumeAsync(CancellationToken stoppingToken)
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

    public event EventHandler<DeadLetterEventArgs>? Store;
}