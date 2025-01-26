using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Errors;

namespace Pot.AspNetCore.Extensions;

internal static class EnrichedErrorExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails GetProblemDetails(this EnrichedError enrichedError)
    {
        var error = enrichedError as ServiceError;

        return error!.ProblemDetails;
    }
}
