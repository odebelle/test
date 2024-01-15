using Identity.Keycloak.Auth.Common;

namespace Identity.Keycloak.Auth.Extension.Middleware;

public class KeycloakProtectionClient(HttpClient client, KeycloakProtectionClientOptions options)
    : IKeycloakProtectionClient
{

    public async Task<bool> VerifyAccessToResource(string resource, string scope, CancellationToken cancellationToken)
    {
        var audience = options.Resource;

        var data = new Dictionary<string, string>()
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:uma-ticket" },
            { "response_mode", "decision" },
            { "audience", audience ?? string.Empty },
            { "permission", $"{resource}#{scope}" }
        };
        var form = new FormUrlEncodedContent(data);
        
        var response = await client.PostAsync(CommonKeycloak.TokenEndpointPath, form, cancellationToken);

        return response.IsSuccessStatusCode;
    }
}