using Identity.Keycloak.Auth.Common;

namespace Identity.Keycloak.Auth.Extension.Administration;

/// <summary>
/// Defines a set of options used to perform Admin HTTP Client calls
/// </summary>
public sealed class KeycloakAdminClientOptions: KeycloakOptions
{
    /// <summary>
    /// Default section name
    /// </summary>
    public const string Section = ConfigurationConstants.ConfigurationPrefix;
}