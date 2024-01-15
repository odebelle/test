using System.Text.Json;
using Identity.Constants;
using Identity.Exceptions;
using Identity.Settings;
using Identity.Tokens;

namespace BusRepositoryApi;

public class KeycloakServiceHandler : DelegatingHandler
{
    private readonly KeycloakSettings _keycloakSettings = new();
    private readonly HttpClient _client;

    public KeycloakServiceHandler(IConfiguration configuration)
    {
        configuration.Bind(nameof(KeycloakSettings), _keycloakSettings);
        _client = new HttpClient();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var kcr = new KeycloakTokenRequest()
        {
            GrantType = KeycloakAccessToken.GrantTypePassword,
            ClientId = _keycloakSettings.ClientId ?? throw new KeycloakException(nameof(KeycloakSettings.ClientId)),
            ClientSecret = _keycloakSettings.ClientSecret ??
                           throw new KeycloakException(nameof(KeycloakSettings.ClientSecret)),
            Username = _keycloakSettings.Username, //?? throw new KeycloakException(nameof(KeycloakSettings.Username)),
            Password = _keycloakSettings.Password, //?? throw new KeycloakException(nameof(KeycloakSettings.Password)),
        };

        var keyValuePairs = new List<KeyValuePair<string, string>>()
        {
            new(KeycloakAccessToken.GrantType, kcr.GrantType),
            new(KeycloakAccessToken.ClientId, kcr.ClientId),
            new(KeycloakAccessToken.ClientSecret, kcr.ClientSecret),
            new(KeycloakAccessToken.Username, kcr.Username),
            new(KeycloakAccessToken.Password, kcr.Password)
        };
        var formUrlEncodedContent = new FormUrlEncodedContent(keyValuePairs);

        var token = await GetTokenAsync(formUrlEncodedContent, cancellationToken);
        request.Headers.Add("Authorization", $"Bearer {token?.AccessToken}");
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<KeycloakTokenResponse?> GetTokenAsync(HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        var result = await _client.PostAsync(_keycloakSettings.BaseUrl, httpContent, cancellationToken);
        var stream = await result.Content.ReadAsStreamAsync(cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<KeycloakTokenResponse>(stream,
            cancellationToken: cancellationToken);

        return token;
    }
}