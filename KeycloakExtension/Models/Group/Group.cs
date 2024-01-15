namespace Identity.Keycloak.Auth.Extension.Models.Group;

/// <summary>
/// Group representation.
/// </summary>
public class Group
{
    public string Id { get; init; } = default!;
    public string? Name { get; init; }
    public string? Path { get; init; }
    public Dictionary<string, string>? ClientRoles { get; init; }
    public string[]? RealmRoles { get; init; }
    public Group[]? SubGroups { get; init; }
    public Dictionary<string, string[]>? Attributes { get; init; }
}