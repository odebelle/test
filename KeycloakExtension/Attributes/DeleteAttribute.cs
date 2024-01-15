namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class DeleteAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => HttpMethod.Delete;
}