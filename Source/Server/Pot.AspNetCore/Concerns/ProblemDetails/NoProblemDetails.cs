namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal sealed class NoProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    public static readonly NoProblemDetails Single = new();
}
