using AllOverIt.Extensions;
using AllOverIt.Patterns.Result;
using Pot.App.Errors;
using Pot.App.Extensions;
using System.Diagnostics;

namespace Pot.AspNetCore.Extensions;

internal static class EnrichedErrorExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this EnrichedError enrichedError)
    {
        var error = enrichedError as ProblemDetailsErrorBase;

        var statusCode = error!.ErrorType switch
        {
            ProblemType.Auth => StatusCodes.Status401Unauthorized,
            ProblemType.NotFound => StatusCodes.Status404NotFound,
            ProblemType.Conflict => StatusCodes.Status409Conflict,
            ProblemType.Constraint => StatusCodes.Status422UnprocessableEntity,
            ProblemType.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
            ProblemType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ProblemType.Server => StatusCodes.Status500InternalServerError,
            _ => throw new UnreachableException($"Unknown problem type: {error.ErrorType}")
        };

        // Note: 'errors' must be an array of objects
        var errors = enrichedError switch
        {
            ProblemDetailsError problemDetailsError => [problemDetailsError.GetErrorDetails()],
            ProblemDetailsBasicError problemDetailsBasicError => [problemDetailsBasicError.GetErrorDetails()],
            ProblemDetailsErrorCollection problemDetailsErrorCollection => problemDetailsErrorCollection.Errors.SelectToArray(error => error.GetErrorDetails()),
            _ => throw new UnreachableException($"Unknown enriched error type: {enrichedError.GetType()}")
        };

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = error.Description,
            Status = statusCode,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errors }
            }
        };
    }
}
