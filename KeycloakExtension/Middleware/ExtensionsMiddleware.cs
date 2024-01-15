using System.Text.Json;
using System.Text.Json.Serialization;
using Identity.Keycloak.Auth.Extension.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Keycloak.Auth.Extension.Middleware;

/// <summary>
/// Token propagation middleware
/// </summary>
public static class ExtensionsMiddleware
{
    /// <summary>
    /// Add token propagation middleware.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddHeaderPropagation(this IHttpClientBuilder builder)
    {
        builder.AddHttpMessageHandler(sp =>
        {
            var contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new AccessTokenPropagationHandler(contextAccessor);
        });

        return builder;
    }

    public static IHttpClientBuilder AddKeycloakProtectionHttpClient(this IServiceCollection services,
        KeycloakProtectionClientOptions options, Action<HttpClient>? configureClient = default)
    {
        services.AddSingleton(options);
        services.AddHttpContextAccessor();

        return services.AddHttpClient<IKeycloakProtectionClient, KeycloakProtectionClient>()
            .ConfigureHttpClient(client =>
            {
                var baseUrl = new Uri(options.KeycloakUrlRealm);
                client.BaseAddress = baseUrl;
                configureClient?.Invoke(client);
            }).AddHeaderPropagation();
    }

}