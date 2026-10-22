using Pot.App.Errors;
using System.Diagnostics;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

/// <summary>
/// The problem types POT publishes, and the one mapping from an application failure category to what a caller sees.
/// </summary>
internal static class ProblemTypes
{
    /// <summary>The conflict returned when the entity tag in an update no longer matches the stored record.</summary>
    internal const string StaleEntityTag = "urn:pot:problem:stale-entity-tag";

    /// <summary>The conflict returned when a value that must be unique already exists.</summary>
    internal const string DuplicateValue = "urn:pot:problem:duplicate-value";

    /// <summary>The conflict a constraint or a race rejected in the database produces.</summary>
    internal const string DataConflict = "urn:pot:problem:data-conflict";

    /// <summary>
    /// Maps an application failure category to the status code it is answered with.
    /// </summary>
    /// <param name="errorType">The category the failure belongs to.</param>
    /// <returns>The status code for the category.</returns>
    /// <exception cref="UnreachableException">The category has no decided status.</exception>
    internal static int ToStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Auth => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Constraint => StatusCodes.Status400BadRequest,
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ErrorType.Server => StatusCodes.Status500InternalServerError,
            _ => throw new UnreachableException($"Unknown error type: {errorType}")
        };
    }

    /// <summary>
    /// Maps the category a conflict reports to the problem type that identifies it to a caller.
    /// </summary>
    /// <param name="errorCode">The category the failure carries.</param>
    /// <returns>The problem type published for the category.</returns>
    /// <exception cref="UnreachableException">The category has no decided type.</exception>
    internal static string ToConflictType(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.StaleEntityTag => StaleEntityTag,
            ErrorCodes.Duplicate => DuplicateValue,
            ErrorCodes.Conflict => DataConflict,
            _ => throw new UnreachableException($"Unknown conflict category: {errorCode}")
        };
    }
}
