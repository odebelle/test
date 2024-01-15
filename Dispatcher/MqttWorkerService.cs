using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using Shared.Models;

namespace Dispatcher;

public abstract class MqttWorkerService : IMqttWorkerService, IDisposable
{
    public IWorkerElements WorkerElements { get; init; } = null!;
    public ILogger<MqttWorkerService> Logger { get; init; } = null!;
    public IConsumerProcessManager? ConsumerProcessManager { get; init; }
    public IList<string>? Topics { get; init; }
    public string MqttBroker { get; init; } = null!;
    public string Route { get; init; } = null!;

    private IMqttClient? _mqttClient;
    private MqttClientDisconnectReason? _mqttClientDisconnectionStatus;
    private MqttClientConnectResult? _mqttBrokerConnectionStatus;
    public IProducerProcessManager? ProducerProcessManager { get; set; }
    private MqttClientSubscribeOptions? _subscribeOptions;
    private readonly MqttFactory _factory = new();


    private IMqttClient MqttClient
    {
        get
        {
            if (_mqttClient is not null)
                return _mqttClient;

            _mqttClient = _factory.CreateMqttClient();

            _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
            _mqttClient.ConnectedAsync += ConnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

            return _mqttClient;
        }
    }

    public async Task InitializeAsync(CancellationToken? token = null)
    {
        token ??= CancellationToken.None;

        if (!MqttClient.IsConnected)
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(MqttBroker)
                .WithClientId(Route)
                .Build();

            _mqttBrokerConnectionStatus = await MqttClient.ConnectAsync(mqttClientOptions, token.Value);
        }
    }

    private MqttClientSubscribeOptions SubscribeOptions
    {
        get
        {
            if (_subscribeOptions is not null)
                return _subscribeOptions;

            var optionsBuilder = _factory.CreateSubscribeOptionsBuilder();

            foreach (var topic in Topics!)
            {
                var t = $"{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}/+/{topic}";
                optionsBuilder.WithTopicFilter(f => f.WithTopic(t));
            }

            _subscribeOptions = optionsBuilder.Build();

            return _subscribeOptions;
        }
    }


    private const string ProducerPausePattern =
        $"^{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}.+{WorkerControllerPath.Producer}.{WorkerAction.Pause}";

    private const string ProducerResumePattern =
        $"^{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}.+{WorkerControllerPath.Producer}.{WorkerAction.Resume}";

    private const string ConsumerPausePattern =
        $"^{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}.+{WorkerControllerPath.Consumer}.{WorkerAction.Pause}";

    private const string ConsumerResumePattern =
        $"^{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}.+{WorkerControllerPath.Consumer}.{WorkerAction.Resume}";

    private Regex ConsumerPauseRegex { get; set; } = new Regex(ConsumerPausePattern, RegexOptions.CultureInvariant);
    private Regex ConsumerResumeRegex { get; set; } = new Regex(ConsumerResumePattern, RegexOptions.CultureInvariant);
    private Regex ProducerPauseRegex { get; set; } = new Regex(ProducerPausePattern, RegexOptions.CultureInvariant);
    private Regex ProducerResumeRegex { get; set; } = new Regex(ProducerResumePattern, RegexOptions.CultureInvariant);

    private async Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        Logger.LogInformation("A message was intercepted with topic: {@Topic}", arg.ApplicationMessage.Topic);

        var workerPattern = $"^{BroMqttRpcClientTopicGenerationStrategy.RequestTopicPrefix}.+{Route}.{WorkerControllerPath.Workers}";
        var workerRegex = new Regex(workerPattern, RegexOptions.Multiline);

        try
        {
            //if (arg.ApplicationMessage.Topic.Contains($"{WorkerControllerPath.Producer}.{WorkerAction.Pause}"))
            if (ProducerPauseRegex.IsMatch(arg.ApplicationMessage.Topic))
                await SendRpcResponse(ProducerProcessManager!.Pause, arg);
            // else if (arg.ApplicationMessage.Topic.Contains($"{WorkerControllerPath.Producer}.{WorkerAction.Resume}"))
            else if (ProducerResumeRegex.IsMatch(arg.ApplicationMessage.Topic))
                await SendRpcResponse(ProducerProcessManager!.Resume, arg);
            // else if (arg.ApplicationMessage.Topic.Contains($"{WorkerControllerPath.Consumer}.{WorkerAction.Pause}"))
            else if (ConsumerPauseRegex.IsMatch(arg.ApplicationMessage.Topic))
                await SendRpcResponse(ConsumerProcessManager!.Pause, arg);
            // else if (arg.ApplicationMessage.Topic.Contains($"{WorkerControllerPath.Consumer}.{WorkerAction.Resume}"))
            else if (ConsumerResumeRegex.IsMatch(arg.ApplicationMessage.Topic))
                await SendRpcResponse(ConsumerProcessManager!.Resume, arg);
            // else if (arg.ApplicationMessage.Topic.Contains($"{_route}.{WorkerControllerPath.Workers}"))
            else if (workerRegex.IsMatch(arg.ApplicationMessage.Topic))
                await SendRpcResponse(SendWorkerElementsAsync, arg);
            else
                Logger.LogInformation("A message with Topic '{@Topic}' was intercepted without being processed!",
                    arg.ApplicationMessage.Topic);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{@Message}", ex.Message);
        }
    }

    private async Task SendRpcResponse(Action actionMethod, MqttApplicationMessageReceivedEventArgs args)
    {
        actionMethod.Invoke();

        await InitializeAsync(CancellationToken.None);

        var arr = System.Text.Encoding.ASCII.GetBytes(WorkerElements.ToJson());

        args.ApplicationMessage.Topic += "/response";
        args.ApplicationMessage.PayloadSegment = new ArraySegment<byte>(arr);

        var result = await MqttClient.PublishAsync(args.ApplicationMessage, CancellationToken.None);
        Logger.LogInformation("Application message was sent with result: {@result}", result);
        Logger.LogInformation("Application message was sent with the following MqttApplicationMessageReceivedEventArgs: \n\t{@args}", args);
        if (result.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Logger.LogWarning(null, "Send to topic '{@topic}' was unsuccessful because: {@ReasonString}", args.ApplicationMessage.Topic,
                result.ReasonString);
        }
    }

    private Task ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _mqttBrokerConnectionStatus = arg.ConnectResult;
        _mqttClient!.SubscribeAsync(SubscribeOptions, CancellationToken.None);

        return Task.CompletedTask;
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _mqttClientDisconnectionStatus = arg.Reason;
        return Task.CompletedTask;
    }

    private async void SendWorkerElementsAsync()
    {
        var topic = $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Consumer}";
        if (WorkerElements.ConsumerElement is not null)
        {
            await SendElementToMqttBrokerAsync(WorkerElements.ConsumerElement, topic, CancellationToken.None);
        }

        topic = $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Producer}";
        if (WorkerElements.ProducerElement is not null)
        {
            await SendElementToMqttBrokerAsync(WorkerElements.ProducerElement, topic, CancellationToken.None);
        }
    }

    public async Task<bool?> SendConsumerElementAsync(ConsumerElement element, CancellationToken cancellationToken)
    {
        try
        {
            const string topic = $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Consumer}";
            return await SendElementToMqttBrokerAsync(element, topic, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{@Message}", e.Message);
            element.MqttClientDisconnectReason = _mqttClientDisconnectionStatus;
            element.MqttClientStatusReasonString = e.Message;
        }

        return false;
    }

    public async Task<bool?> SendProducerElementAsync(IProducerElement element, CancellationToken cancellationToken)
    {
        try
        {
            const string topic = $"/{WorkerControllerPath.Root}/{WorkerControllerPath.Workers}/{WorkerControllerPath.Producer}";
            return await SendElementToMqttBrokerAsync(element, topic, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "{@Message}", e.Message);
            element.MqttClientDisconnectReason = _mqttClientDisconnectionStatus;
            element.MqttClientStatusReasonString = e.Message;
        }

        return false;
    }

    public Task RegisterProducerAsync(string route, IProducerProcessManager processManager)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> SendElementToMqttBrokerAsync(IMqttElement element, string topic,
        CancellationToken cancellationToken)
    {
        var result = new MqttClientPublishResultFactory().Create(new MqttPubAckPacket());

        try
        {
            await InitializeAsync(cancellationToken);
            if (_mqttBrokerConnectionStatus?.ResultCode == MqttClientConnectResultCode.Success && MqttClient.IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(element.ToJson())
                    .Build();

                result = await MqttClient.PublishAsync(applicationMessage, cancellationToken);
                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                    return true;

                Logger.LogWarning(null, "{@ReasonString}", result.ReasonString);
            }
            else
                Logger.LogWarning("Is MqttClient connected? => {@IsConnected}. \nConnection status is : '{@ReasonString}'.",
                    _mqttBrokerConnectionStatus?.ReasonString ?? "UNKNOWN", MqttClient.IsConnected);
        }
        catch (Exception e)
        {
            element.LastErrorMessage = e.Message;
            element.LastError = DateTime.UtcNow;
            Logger.LogWarning(e, "{@Message}", e.Message);
        }


        element.MqttClientPublishReasonCode = result.ReasonCode;
        element.MqttClientStatusReasonString = result.ReasonString;

        return false;
    }

    public void Dispose()
    {
        MqttClient.Dispose();
    }
}