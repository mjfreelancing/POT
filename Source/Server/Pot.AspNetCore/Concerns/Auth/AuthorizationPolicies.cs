namespace Pot.AspNetCore.Concerns.Auth;

/// <summary>
/// Names of the authorization policies that are declared explicitly by the host, for endpoints that do not
/// authorize on a resource:action permission.
/// </summary>
/// <remarks>
/// Permission policies are deliberately absent: <see cref="PermissionAuthorizationPolicyProvider" /> creates a
/// policy on demand for any name it does not recognise, so a permission policy name is the permission string
/// itself (as defined by the Permission enumeration in Pot.Shared). A misspelled name is therefore not a build
/// error - it resolves to a requirement for a permission nobody holds, and the endpoint answers 403 to every
/// caller. Declared policies have no such fallback, so their names must be shared rather than repeated.
/// </remarks>
internal static class AuthorizationPolicies
{
    /// <summary>
    /// Requires an authenticated caller, with no specific permission.
    /// </summary>
    internal const string AuthenticatedUser = "AuthenticatedUser";

    /// <summary>
    /// Requires the platform administration permission, which is granted from deployment configuration rather
    /// than from the caller's roles.
    /// </summary>
    internal const string PlatformManage = "platform:manage";
}
