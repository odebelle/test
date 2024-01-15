namespace Shared.Models;

public class ConfidentialOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Authority { get; set; } = null!;
    public IEnumerable<string>? Scopes { get; set; } = null!;
}