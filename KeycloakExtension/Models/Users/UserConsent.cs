namespace Identity.Keycloak.Auth.Extension.Models.Users;

/// <summary>
/// User consent representation.
/// </summary>
public class UserConsent
{
    public string? ClientId { get; init; }
    public long? CreatedDate { get; init; }
    public long? LastUpdatedDate { get; init; }
    public string[]? GrantedClientScopes { get; init; }
}