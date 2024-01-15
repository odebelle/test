namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Parameter |
                AttributeTargets.Property)] // Property is to allow for form url encoded data
public class QueryAttribute : Attribute
{
    SwaggerCollectionFormat? _collectionFormat;

    public QueryAttribute()
    {
    }

    public QueryAttribute(string delimiter)
    {
        Delimiter = delimiter;
    }

    public QueryAttribute(string delimiter, string prefix)
    {
        Delimiter = delimiter;
        Prefix = prefix;
    }

    public QueryAttribute(string delimiter, string prefix, string format)
    {
        Delimiter = delimiter;
        Prefix = prefix;
        Format = format;
    }

    public QueryAttribute(SwaggerCollectionFormat swaggerCollectionFormat)
    {
        SwaggerCollectionFormat = swaggerCollectionFormat;
    }

    /// <summary>
    /// Used to customize the name of either the query parameter pair or of the form field when form encoding.
    /// </summary>
    /// <seealso cref="Prefix"/>
    public string Delimiter { get; protected set; } = ".";

    /// <summary>
    /// Used to customize the name of the encoded value.
    /// </summary>
    /// <remarks>
    /// Gets combined with <see cref="Delimiter"/> in the format <code>var name = $"{Prefix}{Delimiter}{originalFieldName}"</code>
    /// where <c>originalFieldName</c> is the name of the object property or method parameter.
    /// </remarks>
    /// <example>
    /// <code>
    /// class Form
    /// {
    ///   [Query("-", "dontlog")]
    ///   public string password { get; }
    /// }
    /// </code>
    /// will result in the encoded form having a field named <c>dontlog-password</c>.
    /// </example>
    public string? Prefix { get; protected set; }

    /// <summary>
    /// Used to customize the formatting of the encoded value.
    /// </summary>
    /// <example>
    /// <code>
    /// interface IServerApi
    /// {
    ///   [Get("/expenses")]
    ///   Task addExpense([Query(Format="0.00")] double expense);
    /// }
    /// </code>
    /// Calling <c>serverApi.addExpense(5)</c> will result in a URI of <c>{baseUri}/expenses?expense=5.00</c>.
    /// </example>
    public string? Format { get; set; }

    /// <summary>
    /// Specifies how the collection should be encoded.
    /// </summary>
    public SwaggerCollectionFormat SwaggerCollectionFormat
    {
        // Cannot make property nullable due to Attribute restrictions
        get => _collectionFormat.GetValueOrDefault();
        set => _collectionFormat = value;
    }

    public bool IsCollectionFormatSpecified => _collectionFormat.HasValue;
}