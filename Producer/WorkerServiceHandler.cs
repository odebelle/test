using Shared.Models;

namespace Producer;

public class WorkerServiceHandler : DelegatingHandler
{
    private readonly string _authUri;
    private readonly HttpClient _client;

    public WorkerServiceHandler(IConfiguration configuration)
    {
        _authUri = configuration?.GetValue<string>("") ?? "";
        _client = new HttpClient();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        AuthenticationResult authenticationResult = await GetTokenAsync();
        request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<AuthenticationResult> GetTokenAsync()
    {
        var result = await _client.GetFromJsonAsync<AuthenticationResult>(_authUri);
        return result;
    }
}

public class AuthenticationResult
{
    public string AccessToken { get; set; }
}