namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class OptionsAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => new("OPTIONS");
}