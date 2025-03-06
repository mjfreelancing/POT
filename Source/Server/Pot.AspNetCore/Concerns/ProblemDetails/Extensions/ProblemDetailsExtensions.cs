namespace Pot.AspNetCore.Concerns.ProblemDetails.Extensions;

internal static class ProblemDetailsExtensions
{
    public static bool IsProblem(this Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        return !ReferenceEquals(problemDetails, NoProblemDetails.Single);
    }
}