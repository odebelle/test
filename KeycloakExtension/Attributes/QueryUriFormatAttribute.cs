namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class QueryUriFormatAttribute(UriFormat uriFormat) : Attribute
{
    /// <summary>
    /// Specifies how the Query Params should be encoded.
    /// </summary>
    public UriFormat UriFormat { get; } = uriFormat;
}