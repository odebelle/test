using System.Text.Json.Serialization;

namespace Identity.Tokens;

public class KeycloakTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }
}