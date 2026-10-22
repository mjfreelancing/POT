using AllOverIt.Extensions;
using AllOverIt.Patterns.Result;
using Pot.App.Errors;
using Pot.App.Extensions;
using Pot.AspNetCore.Concerns.ProblemDetails;
using System.Diagnostics;

namespace Pot.AspNetCore.Extensions;

internal static class EnrichedErrorExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this EnrichedError enrichedError)
    {
        var error = enrichedError as ApiErrorBase;
        var errorMessage = (enrichedError as ApiBasicError)?.ErrorMessage;

        var statusCode = ProblemTypes.ToStatusCode(error!.ErrorType);

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = errorMessage,
            Status = statusCode
        };

        if (statusCode == StatusCodes.Status409Conflict)
        {
            // The type, not a field name, is how a caller tells a stale update from a duplicate or a race.
            problemDetails.Type = ProblemTypes.ToConflictType(((ApiBasicError)error).ErrorCode);
        }

        if (statusCode == StatusCodes.Status400BadRequest)
        {
            // A validation problem is the only one that describes individual fields.
            // Every other status states what happened in 'detail' alone.
            var errors = enrichedError switch
            {
                ApiDetailError problemDetailsError => [problemDetailsError.GetErrorDetails()],
                ApiBasicError problemDetailsBasicError => [problemDetailsBasicError.GetErrorDetails()],
                ApiDetailErrorCollection problemDetailsErrorCollection => problemDetailsErrorCollection.Errors.SelectToArray(errorItem => errorItem.GetErrorDetails()),
                _ => throw new UnreachableException($"Unknown enriched error type: {enrichedError.GetType()}")
            };

            problemDetails.Extensions["errors"] = errors;
        }

        return problemDetails;
    }
}
