using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;

namespace Identity.Keycloak.Auth.Common;

/// <summary>
/// keycloak common extensions
/// </summary>
public static class Extensions
{
    private const string ClaimValueType = "JSON";

    /// <summary>
    /// Try get claims from JWT token.
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="resourceAccessCollection"></param>
    /// <returns></returns>
    public static bool TryGetResourceAccessCollection(this IEnumerable<Claim> claims,
        [MaybeNullWhen(false)] out ResourceAccessCollection resourceAccessCollection)
    {
        var claim = claims.SingleOrDefault(s =>
            s.Type.Equals(CommonKeycloak.ResourceAccessClaimType, StringComparison.OrdinalIgnoreCase)
            && s.ValueType.Equals(ClaimValueType, StringComparison.OrdinalIgnoreCase));

        if (claim == null || string.IsNullOrEmpty(claim.Value))
        {
            resourceAccessCollection = default;
            return false;
        }

        var resourcesAccess = JsonSerializer.Deserialize<ResourceAccessCollection>(claim.Value);

        resourceAccessCollection = resourcesAccess!;

        return true;
    }

    /// <summary>
    /// Try get claims from JWT token.
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="resourcesAccess"></param>
    /// <returns></returns>
    public static bool TryGetRealmResource(
        this IEnumerable<Claim> claims,
        [MaybeNullWhen(false)] out ResourceAccess resourcesAccess)
    {
        var claim = claims.SingleOrDefault(x =>
            x.Type.Equals(CommonKeycloak.RealmAccessClaimType, StringComparison.OrdinalIgnoreCase)
            && x.ValueType.Equals(ClaimValueType, StringComparison.OrdinalIgnoreCase));

        if (claim == null || string.IsNullOrEmpty(claim.Value))
        {
            resourcesAccess = default;
            return false;
        }

        resourcesAccess = JsonSerializer.Deserialize<ResourceAccess>(claim.Value)!;
        return true;
    }
}