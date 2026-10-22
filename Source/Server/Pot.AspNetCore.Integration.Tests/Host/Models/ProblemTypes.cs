namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The problem types and default details the API publishes, as a caller reads them.
/// </summary>
/// <remarks>
/// A contract test mirrors the published values rather than importing them, so changing one on either side fails
/// the test that asserts it.
/// </remarks>
public static class ProblemTypes
{
    /// <summary>The conflict an update is answered with when its entity tag no longer matches the stored record.</summary>
    public const string StaleEntityTag = "urn:pot:problem:stale-entity-tag";

    /// <summary>The conflict a rule is answered with when a value must be unique and already exists.</summary>
    public const string DuplicateValue = "urn:pot:problem:duplicate-value";

    /// <summary>The conflict a constraint or a race rejected in the database produces.</summary>
    public const string DataConflict = "urn:pot:problem:data-conflict";

    /// <summary>The detail a 401 carries when the failure supplies no message of its own.</summary>
    public const string AuthenticationRequiredDetail = "Authentication is required.";

    /// <summary>The detail a 404 carries when the failure supplies no message of its own.</summary>
    public const string NotFoundDetail = "The requested resource was not found.";

    /// <summary>The detail a 405 carries when the failure supplies no message of its own.</summary>
    public const string MethodNotAllowedDetail = "The requested method is not allowed for this resource.";

    /// <summary>The detail a 500 carries when the failure supplies no message of its own.</summary>
    public const string UnexpectedErrorDetail = "An unexpected error occurred.";
}
