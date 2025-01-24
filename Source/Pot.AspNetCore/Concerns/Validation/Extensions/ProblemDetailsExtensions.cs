using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

internal static class ProblemDetailsExtensions
{
    public static bool IsProblem(this Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        return !ReferenceEquals(problemDetails, NoProblemDetails.Single);
    }
}