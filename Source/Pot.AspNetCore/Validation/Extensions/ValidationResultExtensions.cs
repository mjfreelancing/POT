using AllOverIt.Extensions;
using FluentValidation.Results;
using Pot.AspNetCore.ProblemDetails;
using System.Net;

namespace Pot.AspNetCore.Validation.Extensions;

internal static class ValidationResultExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var errorDetails = validationResult.Errors.SelectToArray(error => new ProblemDetailsError
        {
            PropertyName = error.PropertyName,
            ErrorCode = error.ErrorCode,
            AttemptedValue = error.AttemptedValue,
            ErrorMessage = error.ErrorMessage
        });

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.UnprocessableEntity,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errorDetails }
            }
        };
    }
}

