namespace Identity.Keycloak.Auth.Extension.Attributes;

/// <summary>
/// Used to store the value in HttpRequestMessage.Properties for further processing in a custom DelegatingHandler.
/// If a string is supplied to the constructor then it will be used as the key in the HttpRequestMessage.Properties dictionary.
/// If no key is specified then the key will be defaulted to the name of the parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class PropertyAttribute : Attribute
{
    public PropertyAttribute()
    {
    }

    public PropertyAttribute(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Specifies the key under which to store the value on the HttpRequestMessage.Properties dictionary.
    /// </summary>
    public string? Key { get; }
}