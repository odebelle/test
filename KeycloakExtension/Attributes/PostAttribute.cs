namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PostAttribute(string path) : HttpMethodAttribute(path)
{
    public override HttpMethod Method => HttpMethod.Post;
}