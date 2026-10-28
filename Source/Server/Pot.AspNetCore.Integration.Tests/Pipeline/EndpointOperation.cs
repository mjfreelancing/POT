using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// One documented operation, named by the method it answers and the shape of its path.
/// </summary>
/// <param name="Method">The HTTP method, upper case.</param>
/// <param name="Path">The path with any route constraint removed, so a documented path compares with a mapped route.</param>
public sealed partial record EndpointOperation(string Method, string Path)
{
    [GeneratedRegex(@"\{([^}:]+):[^}]+\}")]
    private static partial Regex RouteConstraintPattern();

    /// <summary>
    /// Creates an operation from the method and path shape the contract documents or a route declares.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The path, with or without a route constraint such as <c>{id:guid}</c>.</param>
    /// <returns>The operation, normalised so the two sources compare equal.</returns>
    public static EndpointOperation Create(string method, string path)
    {
        return new EndpointOperation(method.ToUpperInvariant(), Normalise(path));
    }

    /// <summary>
    /// The operation as a reviewer reads it in a failure message.
    /// </summary>
    /// <returns>The method followed by the path shape.</returns>
    public override string ToString()
    {
        return $"{Method} {Path}";
    }

    private static string Normalise(string path)
    {
        var trimmed = path.Trim().Trim('/');
        var withoutConstraint = RouteConstraintPattern().Replace(trimmed, "{$1}");

        return $"/{withoutConstraint}";
    }
}
