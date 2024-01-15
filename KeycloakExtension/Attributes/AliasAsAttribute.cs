namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class AliasAsAttribute(string name) : Attribute
{
    public string Name { get; protected set; } = name;
}