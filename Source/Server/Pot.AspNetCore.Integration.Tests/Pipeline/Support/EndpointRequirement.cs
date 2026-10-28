using Pot.AspNetCore.Concerns.Auth;

namespace Pot.AspNetCore.Integration.Tests.Pipeline.Support;

/// <summary>
/// What an endpoint declares about who may call it, read from the endpoint itself rather than from a second table
/// of intended outcomes.
/// </summary>
/// <param name="AllowsAnonymous">Whether the endpoint is marked <c>AllowAnonymous</c>.</param>
/// <param name="Policies">
/// The policies the endpoint requires. A permission policy carries the permission string, so a policy name that is
/// not <see cref="AuthorizationPolicies.AuthenticatedUser" /> is a permission the caller must hold.
/// </param>
public sealed record EndpointRequirement(bool AllowsAnonymous, IReadOnlyList<string> Policies)
{
    /// <summary>
    /// Whether a caller holding the given permissions satisfies the declaration.
    /// </summary>
    /// <param name="permissions">The permissions the caller holds.</param>
    /// <returns><see langword="true" /> when the endpoint would let the caller through.</returns>
    public bool Allows(IReadOnlySet<string> permissions)
    {
        if (AllowsAnonymous)
        {
            return true;
        }

        return Policies.All(policy => policy == AuthorizationPolicies.AuthenticatedUser || permissions.Contains(policy));
    }

    /// <summary>
    /// Describes the declaration the way a failure message should read it.
    /// </summary>
    /// <returns>The declared requirement in words.</returns>
    public string Describe()
    {
        if (AllowsAnonymous)
        {
            return "allows anonymous callers";
        }

        var required = Policies.Count == 0
            ? AuthorizationPolicies.AuthenticatedUser
            : string.Join(" and ", Policies);

        return $"requires {required}";
    }
}
