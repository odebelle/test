namespace Shared.Clients;

public class BrokerClient : IBrokerClient
{
    public Task<QueueDetail?> GetQueueDetailsAsync(string vhost, string queueName, QueueDetailsBodyRequest? options)
    {
        throw new NotImplementedException();
    }
}