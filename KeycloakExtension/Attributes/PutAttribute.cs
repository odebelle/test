namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PutAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => HttpMethod.Put;
}