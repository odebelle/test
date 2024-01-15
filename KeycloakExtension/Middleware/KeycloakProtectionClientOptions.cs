using Identity.Keycloak.Auth.Common;

namespace Identity.Keycloak.Auth.Extension.Middleware;

public sealed class KeycloakProtectionClientOptions : KeycloakOptions
{
    /// <summary>
    /// Default section
    /// </summary>
    public const string Section = ConfigurationConstants.ConfigurationPrefix;
}