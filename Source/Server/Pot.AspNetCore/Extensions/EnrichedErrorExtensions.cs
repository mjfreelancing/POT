using AllOverIt.Patterns.Result;
using Pot.App.Errors;
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
            // A validation problem is the only one that describes individual fields: the messages live in the map
            // and there is no 'detail' to read instead.
            var failures = enrichedError switch
            {
                ApiDetailError problemDetailsError => [new ValidationErrors.FieldFailure(problemDetailsError.PropertyName, problemDetailsError.ErrorMessage)],
                ApiBasicError problemDetailsBasicError => [new ValidationErrors.FieldFailure(string.Empty, problemDetailsBasicError.ErrorMessage)],
                ApiDetailErrorCollection problemDetailsErrorCollection => problemDetailsErrorCollection.Errors
                    .Select(errorItem => new ValidationErrors.FieldFailure(errorItem.PropertyName, errorItem.ErrorMessage))
                    .ToArray(),
                _ => throw new UnreachableException($"Unknown enriched error type: {enrichedError.GetType()}")
            };

            problemDetails.Detail = null;
            problemDetails.Title = ValidationErrors.Title;
            problemDetails.Extensions[ValidationErrors.Extension] = ValidationErrors.ToMap(failures);
        }

        return problemDetails;
    }
}
