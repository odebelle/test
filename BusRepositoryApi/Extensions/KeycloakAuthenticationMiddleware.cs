using System.Security.Claims;
using System.Text.Json;

namespace BusRepositoryApi.Extensions;

public class KeycloakAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var client = configuration.GetValue<string>("KeycloakSettings:resource") ?? "";
        if (string.IsNullOrEmpty(client))
            await next(context);
        else
        {
            if (context.User.Identity is not ClaimsIdentity identity)
                await next(context);
            else
            {
                var claims = context.User.Claims.ToList();
                var value = context.User.FindFirstValue(KeycloakConstants.ResourceAccess) ?? "{}";
                
                var resource = JsonSerializer.Deserialize<Dictionary<string, KeycloakAccess>>(value);

                if (resource!.TryGetValue(client, out var worker))
                {
                    claims.AddRange(worker.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
                    if (resource.TryGetValue("account", out worker))
                        claims.AddRange(worker.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
                }
            }
        }

        await next(context);
    }
}