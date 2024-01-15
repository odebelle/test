namespace Shared.Models;

public class RemoteOperator
{
    public string BaseUrl { get; set; } = null!;
    public string? Route { get; set; }
    public string? Scope { get; set; }
}