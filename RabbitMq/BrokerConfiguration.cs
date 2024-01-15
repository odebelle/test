using RabbitMQ.Client;
using Shared;

namespace RabbitMq;

public class BrokerConfiguration
{
    private static BrokerOptions? _options = null;
    private static readonly BrokerConfiguration Instance = new BrokerConfiguration();

    private static BrokerConfiguration GetInstance(BrokerOptions options)
    {
        if (_options is not null) 
            return Instance;
        
        _options = options;

        if (!_options.TryGetValue(nameof(BrokerConfiguration.HostName), out var hostName))
            throw new MissingFieldException(nameof(BrokerConfiguration), nameof(BrokerConfiguration.HostName));
        if (!_options.TryGetValue(nameof(BrokerConfiguration.VirtualHost), out var virtualHost))
            throw new MissingFieldException(nameof(BrokerConfiguration), nameof(BrokerConfiguration.HostName));

        Instance.HostName = $"{hostName}";
        Instance.VirtualHost = $"{virtualHost}";
        Instance.AuthenticationType = !options.TryGetValue(nameof(BrokerConfiguration.HostName), out var authType)
            ? AuthenticationType.UserSecret
            : Enum.Parse<AuthenticationType>($"{authType}");
        if (Instance.AuthenticationType == AuthenticationType.UserSecret)
        {
            if (options.TryGetValue(nameof(BrokerConfiguration.UserName), out var userName))
                Instance.UserName = $"{userName}";
            else
                throw new MissingFieldException(nameof(BrokerConfiguration), nameof(UserName));
            if (options.TryGetValue(nameof(BrokerConfiguration.Secret), out var secret))
                Instance.Secret = $"{secret}";
            else
                throw new MissingFieldException(nameof(BrokerConfiguration), nameof(Secret));
        }

        if (options.TryGetValue(nameof(BrokerConfiguration.ChannelContinuationTimeOut), out var timeout))
            Instance.ChannelContinuationTimeOut = TimeSpan.Parse($"{timeout}");

        if (options.TryGetValue(nameof(BrokerConfiguration.DefaultExchangeName), out var defaultXName))
            Instance.DefaultExchangeName = $"{defaultXName}";
        if (options.TryGetValue(nameof(BrokerConfiguration.AllowDeclaration), out var allowDeclaration))
            Instance.AllowDeclaration = bool.Parse($"{allowDeclaration}");
        if (options.TryGetValue(nameof(BrokerConfiguration.DefaultConsumerArguments), out var defaultConsumerArgs))
            Instance.DefaultConsumerArguments = defaultConsumerArgs as IDictionary<string, object>;

        throw new NotImplementedException();

    }


    private IList<AmqpTcpEndpoint>? _endpoints;
    public string? HostName { get; set; }
    public string? VirtualHost { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
    public string? UserName { get; set; }
    public string? Secret { get; set; }
    public TimeSpan? ChannelContinuationTimeOut { get; set; } = null!;
    public string? DefaultExchangeName { get; set; }
    public bool AllowDeclaration { get; set; }
    public IDictionary<string, object>? DefaultConsumerArguments { get; set; }

    public IList<AmqpTcpEndpoint> Endpoints
    {
        get
        {
            _endpoints = HostName?.Split(',').Select(s => new AmqpTcpEndpoint(s)).ToList();
            return _endpoints ?? throw new ArgumentException($"{nameof(HostName)} cannot be null or empty.");
        }
    }
}