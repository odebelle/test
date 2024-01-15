using Microsoft.AspNetCore.Authorization;

namespace BusRepositoryApi.Extensions;

/// <summary>
/// Policies defined for the application.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Name of the policy to check if a user can unpublish posts.
    /// </summary>
    public const string CanUnpublish = "CanUnpublish";

    public const string DispatcherOperator = "Dispatcher-Operator";
    public const string DispatcherWorker = "Dispatcher-Worker";

    public static void DispatcherOperatorPolicy(AuthorizationPolicyBuilder builder)
    {
        builder.RequireClaim(KeycloakConstants.ResourceAccess);
        builder.RequireRole(Roles.Operator);
    }

    public static void DispatcherWorkerPolicy(AuthorizationPolicyBuilder builder)
    {
        builder.RequireClaim(KeycloakConstants.ResourceAccess);
        builder.RequireRole(Roles.Worker);
    }
}

public static class Roles
{
    public const string Operator = "operator-role";
    public const string Worker = "worker-role";
}