namespace Shared.Models;

public class BrokerHandler : DelegatingHandler
{
    // TODO: Implement certificate authentication
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", $"Basic cXVldWVBZ2VudDpxdWV1ZUFnZW50");
        return await base.SendAsync(request, cancellationToken);
    }
}