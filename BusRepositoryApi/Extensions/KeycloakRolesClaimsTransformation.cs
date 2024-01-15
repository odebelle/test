using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace BusRepositoryApi.Extensions;

public class KeycloakRolesClaimsTransformation(IConfiguration configuration) : IClaimsTransformation
{
    private readonly string _client = configuration.GetValue<string>("KeycloakSettings:resource") ?? "";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var result = principal.Clone();
        if (result.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(result);
        }

        var value = principal.FindFirstValue(KeycloakConstants.ResourceAccess);
        if(string.IsNullOrEmpty(value))
            return Task.FromResult(result);
        var resource = JsonSerializer.Deserialize<Dictionary<string, KeycloakAccess>>(value);

        if (!resource!.TryGetValue(_client, out var worker)) 
            return Task.FromResult(result);
        
        identity.AddClaims(worker.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        if (resource.TryGetValue(EsbClaimType.Account ,out worker))
            identity.AddClaims(worker.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return Task.FromResult(result);
    }
}