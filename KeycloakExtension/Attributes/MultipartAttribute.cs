namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MultipartAttribute(string boundaryText = "----MyGreatBoundary") : Attribute
{
    public string BoundaryText { get; private set; } = boundaryText;
}