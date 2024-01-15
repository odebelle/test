using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
using Persistence;
using Shared.Enums;
using Shared.Models;

namespace ServerSideOperator.Services;

public sealed class MqttService(ILogger<MqttService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    : IDisposable, IMqttService
{
    private readonly string _mqttBroker = configuration.GetValue<string>("MqttBroker")!;

    private IManagedMqttClient? _mqttClient;
    private IEnumerable<Dispatch>? _proxies;
    private MqttFactory _mqttFactory = null!;
    private MqttRpcClientOptions? _rpcOptions;
    private readonly MqttQualityOfServiceLevel _qos = MqttQualityOfServiceLevel.AtMostOnce;
    private Dispatch _currentProxy = null!;

    public event Action? OnProducerChange;
    public event Action? OnConsumerChange;
    public event EventHandler<DaemonMessageEventArgs>? OnDaemonChange;

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        logger.LogWarning(arg.Exception, "Mqtt client disconnected. {@ReasonString}", arg.ReasonString);
        return Task.CompletedTask;
    }


    public async Task SendToTopic(string topic, string? payload = null)
    {
        try
        {
            var pl = payload?.SerializeToUtf8Bytes() ?? Enumerable.Empty<byte>().ToArray();
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBroker)
                .WithCredentials("queueAgent", "queueAgent")
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions);

            using var mqttRpcClient = mqttFactory.CreateMqttRpcClient(mqttClient, RpcOptions);

            logger.LogInformation("A MQTT message with topic '{@topic}' was sent.", topic);

            var response = await mqttRpcClient.ExecuteAsync(TimeSpan.FromSeconds(15), topic, pl, _qos);

            var result = JsonSerializer.Deserialize<WorkerElements>(Encoding.UTF8.GetString(response));

            if (result is not null)
                await SetElementsAsync(result);
        }
        catch (Exception exception)
        {
            await SetElementsAsync(GetElements(_currentProxy));
            logger.LogError(exception, "message on error with {@Topic}\n{@Message}",
                topic, exception.Message);
            throw;
        }
    }

    private MqttRpcClientOptions RpcOptions =>
        _rpcOptions ??= new MqttRpcClientOptionsBuilder()
            .WithTopicGenerationStrategy(new BroMqttRpcClientTopicGenerationStrategy())
            .Build();

    private Task OnConnectingFailedAsync(ConnectingFailedEventArgs arg)
    {
        logger.LogInformation("Unable to connect mqtt client. {@Message}", arg.Exception.Message);
        logger.LogInformation("{@ConnectResult}", arg.ConnectResult);
        return Task.CompletedTask;
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        logger.LogInformation("Mqtt client connected.");
        logger.LogInformation("{@ConnectResult}", arg.ConnectResult);

        _mqttClient.SubscribeAsync(Daemon.Display);
        _mqttClient.SubscribeAsync($"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/#");

        return Task.CompletedTask;
    }

    private Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        try
        {
            switch (arg.ApplicationMessage.Topic)
            {
                case $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Consumer}":
                {
                    var consumerElement = JsonSerializer.Deserialize<ConsumerElement>(arg.ApplicationMessage.ConvertPayloadToString());
                    if (consumerElement != null)
                        SetConsumerAsync(consumerElement);
                    break;
                }
                case $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Producer}":
                {
                    var producerElement = JsonSerializer.Deserialize<ProducerElement>(arg.ApplicationMessage.ConvertPayloadToString());
                    if (producerElement != null)
                        SetProducerAsync(producerElement);
                    break;
                }
                case Daemon.Display:
                {
                    var daemonMessage = JsonSerializer.Deserialize<DaemonMessage>(arg.ApplicationMessage.ConvertPayloadToString());
                    if (daemonMessage != null)
                        OnDaemonChanged(daemonMessage);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{@Message}", ex.Message);
        }

        return Task.CompletedTask;
    }

    public void OnDaemonChanged(DaemonMessage daemonMessage)
    {
        OnDaemonChange?.Invoke(this, new DaemonMessageEventArgs() { Message = daemonMessage });
    }

    public List<IConsumerElement> Consumers { get; } = new();
    public List<IProducerElement> Producers { get; } = new();

    public Task ConnectAsync(CancellationToken? cancellationToken)
    {
        if (_mqttClient is null)
        {
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateManagedMqttClient();

            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.ConnectingFailedAsync += OnConnectingFailedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += MessageReceivedAsync;
        }

        if (_mqttClient.IsStarted)
            return Task.CompletedTask;

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttBroker)
            .WithClientId($"BusRemoteOperator_{Guid.NewGuid()}")
            .WithCredentials("queueAgent","queueAgent")
            .Build();

        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(15))
            .Build();

        return _mqttClient.StartAsync(managedMqttClientOptions);
    }

    public async Task<List<string>> PingWorkersAsync()
    {
        var result = Enumerable.Empty<string>().ToList();
        return await Task.FromResult(result);
        // using (var scope = serviceProvider.CreateScope())
        // {
        //     await using (var ctx = scope.ServiceProvider.GetRequiredService<BusRemoteOperatorContext>())
        //     {
        //         _proxies = ctx.Dispatch
        //             .Include(i => i.Producer)
        //             .Include(i => i.Consumer)
        //             .Where(w => w.IsWorker == true && w.Enabled)
        //             .ToArray();
        //     }
        // }

        var errorList = new List<string>();

        foreach (var proxy in _proxies)
        {
            try
            {
                _currentProxy = proxy;
                await SendToTopic($"{proxy.Name}_{WorkerControllerPath.Workers}");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "{@Message}", e.Message);
                // TODO: INFORM SOME BAD NEWS
                errorList.Add(e.Message);
            }
        }

        return errorList;
    }

    private WorkerElements GetElements(Dispatch dispatch)
    {
        WorkerElements response = new();

        if (dispatch.Consumer is not null)
        {
            response.ConsumerElement = new()
            {
                Host = dispatch.Cluster,
                RunningStatus = RunningStatus.Unknown,
                Group = dispatch.Subject ?? dispatch.Name,
                Route = dispatch.Name,
                TopicName = dispatch.Consumer.Topic,
                Information = Constants.Unreachable
            };
        }

        if (dispatch.Producer is not null)
        {
            response.ProducerElement = new ProducerElement()
            {
                Host = dispatch.Cluster,
                RunningStatus = RunningStatus.Unknown,
                Group = dispatch.Subject ?? dispatch.Name,
                Route = dispatch.Name,
                TopicName = dispatch.Producer.Topic,
                Information = Constants.Unreachable
            };
        }

        return response;
    }

    public Task SetElementsAsync(WorkerElements elements)
    {
        lock (this)
        {
            if (elements.ConsumerElement is not null)
                UpdateConsumerCollection(elements.ConsumerElement);
            if (elements.ProducerElement is not null)
                UpdateProducerCollection(elements.ProducerElement);
        }

        return Task.CompletedTask;
    }

    public Task SetProducerAsync(IProducerElement element)
    {
        UpdateProducerCollection(element);

        return Task.CompletedTask;
    }

    public Task SetConsumerAsync(IConsumerElement element)
    {
        UpdateConsumerCollection(element);

        return Task.CompletedTask;
    }

    private void UpdateConsumerCollection(IConsumerElement received)
    {
        IConsumerElement? consumer = null;

        if (Consumers.Any())
            consumer = Consumers.SingleOrDefault(s => s.Equals(received));

        if (consumer is not null)
        {
            consumer.UpdateMe(received);
        }
        else
        {
            Consumers.Add(received);
        }

        OnConsumerChange?.Invoke();
    }

    private void UpdateProducerCollection(IProducerElement received)
    {
        IProducerElement? producer = null;

        if (Producers.Any())
            producer = Producers.SingleOrDefault(s => s.Equals(received));

        if (producer is not null)
            producer.UpdateMe(received);
        else
            Producers.Add(received);

        OnProducerChange?.Invoke();
    }

    public void Dispose()
    {
        _mqttClient?.Dispose();
    }
}