using System.Text.Json.Serialization;

namespace Identity.Keycloak.Auth.Common;

public class ResourceAccess
{
    /// <summary>
    /// Get Roles
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; init; } = [];
}