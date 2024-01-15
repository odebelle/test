namespace Identity.Keycloak.Auth.Extension.Attributes;

public abstract class HttpMethodAttribute(string path) : Attribute
{
    public abstract HttpMethod Method { get; }

    public virtual string Path
    {
        get => path;
        protected set => path = value;
    }
}