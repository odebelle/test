using MQTTnet.Extensions.Rpc;

namespace Shared.Models;

public sealed class BroMqttRpcClientTopicGenerationStrategy : IMqttRpcClientTopicGenerationStrategy
{
    public const string RequestTopicPrefix = "BRO";
    public const string ResponseTopicSuffix = "/response";

    public MqttRpcTopicPair CreateRpcTopics(TopicGenerationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!context.MethodName.Contains('_'))
        {
            throw new ArgumentException("The method name must contain _");
        }

        var requestTopic = $"{RequestTopicPrefix}/{Guid.NewGuid():N}/{context.MethodName}";
        var responseTopic = requestTopic + ResponseTopicSuffix;

        return new MqttRpcTopicPair
        {
            RequestTopic = requestTopic,
            ResponseTopic = responseTopic
        };
    }
}