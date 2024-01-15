namespace Identity.Keycloak.Auth.Extension.Models.Users;

/// <summary>
/// Federated identity representation.
/// </summary>
public class FederatedIdentity
{
    public string? IdentityProvider { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
}