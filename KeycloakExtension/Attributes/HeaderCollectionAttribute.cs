namespace Identity.Keycloak.Auth.Extension.Attributes;

/// <summary>
/// Allows you provide a Dictionary of headers to be added to the request.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderCollectionAttribute : Attribute
{
}