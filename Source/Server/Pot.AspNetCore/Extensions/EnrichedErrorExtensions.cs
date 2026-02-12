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
        var error = enrichedError as ApiErrorBase;

        var statusCode = error!.ErrorType switch
        {
            ErrorType.Auth => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Constraint => StatusCodes.Status422UnprocessableEntity,
            ErrorType.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ErrorType.Server => StatusCodes.Status500InternalServerError,
            _ => throw new UnreachableException($"Unknown problem type: {error.ErrorType}")
        };

        // Note: 'errors' must be an array of objects
        var errors = enrichedError switch
        {
            ApiDetailError problemDetailsError => [problemDetailsError.GetErrorDetails()],
            ApiBasicError problemDetailsBasicError => [problemDetailsBasicError.GetErrorDetails()],
            ApiDetailErrorCollection problemDetailsErrorCollection => problemDetailsErrorCollection.Errors.SelectToArray(error => error.GetErrorDetails()),
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
