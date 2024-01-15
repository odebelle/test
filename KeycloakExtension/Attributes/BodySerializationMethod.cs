namespace Identity.Keycloak.Auth.Extension.Attributes;

public enum BodySerializationMethod
{
    /// <summary>
    /// Encodes everything using the ContentSerializer in RefitSettings except for strings. Strings are set as-is
    /// </summary>
    Default,

    /// <summary>
    /// Json encodes everything, including strings
    /// </summary>
    [Obsolete("Use BodySerializationMethod.Serialized instead", false)]
    Json,

    /// <summary>
    /// Form-UrlEncode's the values
    /// </summary>
    UrlEncoded,

    /// <summary>
    /// Encodes everything using the ContentSerializer in RefitSettings
    /// </summary>
    Serialized
}