namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PatchAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => new("PATCH");
}