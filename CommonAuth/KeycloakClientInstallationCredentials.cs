namespace Identity.Keycloak.Auth.Common;

/// <summary>
/// Keycloak client credentials
/// </summary>
public class KeycloakClientInstallationCredentials
{
    /// <summary>
    /// Secret
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}