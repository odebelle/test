namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HeadAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => HttpMethod.Head;
}