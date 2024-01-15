namespace Shared.Clients;

public interface IBrokerClient
{
    Task<QueueDetail?> GetQueueDetailsAsync(string vhost, string queueName, QueueDetailsBodyRequest? options);
}