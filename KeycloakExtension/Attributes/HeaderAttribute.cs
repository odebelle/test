namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderAttribute(string header) : Attribute
{
    public string Header { get; } = header;
}