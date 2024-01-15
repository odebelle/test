using Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Models;

namespace RabbitMq;

public static class Extensions
{
    /// <summary>
    /// SetBasicConnectionProperties
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static ConnectionFactory SetBasicConnectionProperties(this ConnectionFactory factory,
        BrokerConfiguration configuration)
    {
        factory.HostName = configuration.HostName;
        factory.VirtualHost = configuration.VirtualHost;
        factory.AutomaticRecoveryEnabled = true;

        switch (configuration.AuthenticationType)
        {
            case AuthenticationType.UserSecret:
                factory.UserName = configuration.UserName;
                factory.Password = configuration.Secret;
                break;
            case AuthenticationType.Certificate:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return factory;
    }
}