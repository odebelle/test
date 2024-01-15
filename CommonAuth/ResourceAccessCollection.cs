namespace Identity.Keycloak.Auth.Common;

public class ResourceAccessCollection : Dictionary<string, ResourceAccess>
{
    /// <summary>
    /// Constructor (with ignore case comparison)
    /// </summary>
    public ResourceAccessCollection() : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}