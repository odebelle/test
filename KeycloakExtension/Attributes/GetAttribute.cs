namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class GetAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => HttpMethod.Get;
}