using Identity.Keycloak.Auth.Extension.Attributes;
using Identity.Keycloak.Auth.Extension.Constants;

namespace Identity.Keycloak.Auth.Extension.Administration;

/// <summary>
/// Realm management
/// </summary>
public interface IKeycloakRealmClient
{
    /// <summary>
    /// Get realm
    /// </summary>
    /// <param name="realm"></param>
    /// <returns></returns>
    [Get(KeycloakClientApiConstants.GetRealm)]
    [Headers("Accept: application/json")]
    Task<string> GetRealm(string realm);
}