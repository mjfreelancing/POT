using AllOverIt.Extensions;
using FluentValidation.Results;
using Pot.App.Concerns.Validation;
using Pot.App.Extensions;
using System.Net;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

internal static class ValidationResultExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var errorDetails = validationResult.ToProblemDetailsErrors();

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.UnprocessableEntity,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errorDetails.SelectToArray(error => error.GetErrorDetails()) }
            }
        };
    }
}

