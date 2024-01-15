using System.Collections.Immutable;
using Identity.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BusRepositoryApi.Extensions;

public static class Extensions
{
    public static IApplicationBuilder UseKeycloakAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull((object)app, nameof(app));
        app.Properties["__AuthenticationMiddlewareSet"] = (object)true;
        return app.UseMiddleware<KeycloakAuthenticationMiddleware>();
    }

    public static void AddKeycloakSettings(this WebApplicationBuilder builder)
    {
        var keycloakSettings = builder.Configuration.GetSection(nameof(KeycloakSettings));

        builder.Services.Configure<KeycloakSettings>(keycloakSettings);
    }

    public static void AddKeycloakAuthorization(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;

        builder.Services
            .AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:8080/realms/bro";
                options.SaveToken = false;
                options.RequireHttpsMetadata = false; // set true for production

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:8080/realms/bro"
                };
            });
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
        builder.Services.AddAuthorization(opt =>
        {
            opt.AddPolicy(Policies.DispatcherOperator, Policies.DispatcherOperatorPolicy);
        });
    }
}

public class WorkerRequirement : IAuthorizationRequirement
{
    public WorkerRequirement(string esbRole)
    {
        //throw new NotImplementedException();
    }
}

public class WorkerAuthorizeAttribute : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }

    public string? Client { get; set; }
}