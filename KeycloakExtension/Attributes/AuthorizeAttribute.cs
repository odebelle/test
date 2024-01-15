namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class AuthorizeAttribute(string scheme = "Bearer") : Attribute
{
    public string Scheme { get; } = scheme;
}