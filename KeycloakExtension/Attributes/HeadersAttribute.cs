namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public class HeadersAttribute(params string[] headers) : Attribute
{
    public string[] Headers { get; } = headers ?? Array.Empty<string>();
}