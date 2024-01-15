using System.Text.Json.Serialization;

namespace BusRepositoryApi.Extensions;

public class KeycloakAccess
{
    [JsonPropertyName("roles")] public List<string> Roles { get; set; }
}