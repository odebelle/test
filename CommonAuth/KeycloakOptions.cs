using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.Configuration;

namespace Identity.Keycloak.Auth.Common;

/// <summary>
/// Options for keycloak
/// </summary>
/// <remarks>
/// See "/.well-known/openid-configuration"
/// </remarks>
public class KeycloakOptions
{
    private string? _authServerUrl = null!;
    private bool? _verifyTokenAudience;
    private TimeSpan? _tokenClockSkew;
    private string? _sslRequired;

    /// <summary>
    /// Authorization server URL
    /// </summary>
    /// <example>
    /// "auth-server-url": "http://localhost:8080/"
    /// </example>
    [ConfigurationKeyName("auth-server-url")]
    public string AuthServerUrl
    {
        get => _authServerUrl ?? this.AuthServerUrl2;
        set => _authServerUrl = value;
    }

    [ConfigurationKeyName("AuthServerUrl")]
    private string AuthServerUrl2 { get; set; } = default!;

    /// <summary>
    /// Keycloak realm
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak resource
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Audience
    /// </summary>
    [ConfigurationKeyName("verify-token-audience")]
    public bool? VerifyTokenAudience
    {
        get => _verifyTokenAudience ?? this.VerifyTokenAudience2;
        set => _verifyTokenAudience = value;
    }
    [ConfigurationKeyName("VerifyTokenAudience")]
    private bool? VerifyTokenAudience2 { get; set; }

    /// <summary>
    /// Token timespan
    /// </summary>
    [ConfigurationKeyName("token-clock-skew")]
    public TimeSpan? TokenClockSkew
    {
        get => _tokenClockSkew ?? this.TokenClockSkew2 ?? TimeSpan.Zero;
        set => _tokenClockSkew = value;
    }
    [ConfigurationKeyName("TokenClockSkew")]
    private TimeSpan? TokenClockSkew2 { get; set; }

    /// <summary>
    /// Require HTTPS
    /// </summary>
    [ConfigurationKeyName("ssl-required")]
    public string SslRequired
    {
        get => _sslRequired??this.SslRequired2;
        set => _sslRequired = value;
    }
    [ConfigurationKeyName("ssl-Required")] private string SslRequired2 { get; set; } = default!;

    /// <summary>
    /// Realm Url
    /// </summary>
    public string KeycloakUrlRealm => $"{NormalizeUrl(AuthServerUrl)}/realms/{Realm}";
    private static string NormalizeUrl(string url) => !url.EndsWith('/') ? url : url.TrimEnd('/');

    /// <summary>
    /// RolesClaimTransformationSource
    /// </summary>
    [ConfigurationKeyName("RolesSource")]
    public RolesClaimTransformationSource RolesSource { get; set; } = RolesClaimTransformationSource.ResourceAccess;
}