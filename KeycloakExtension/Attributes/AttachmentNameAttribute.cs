namespace Identity.Keycloak.Auth.Extension.Attributes;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
[Obsolete(
    "Use Refit.StreamPart, Refit.ByteArrayPart, Refit.FileInfoPart or if necessary, inherit from Refit.MultipartItem",
    false)]
public class AttachmentNameAttribute(string name) : Attribute
{
    public string Name { get; protected set; } = name;
}