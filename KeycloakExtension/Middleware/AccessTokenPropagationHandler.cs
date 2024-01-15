using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Identity.Keycloak.Auth.Extension.Middleware;

/// <summary>
/// Delegating handler to propagate headers
/// </summary>
/// <param name="contextAccessor"></param>
public class AccessTokenPropagationHandler(IHttpContextAccessor contextAccessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_contextAccessor.HttpContext == null)
            return await base.SendAsync(request, cancellationToken);

        var httpContext = _contextAccessor.HttpContext;
        var token = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");

        if (!StringValues.IsNullOrEmpty(token))
            request.Headers.Authorization =
                new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);

        return await base.SendAsync(request, cancellationToken);
    }
}