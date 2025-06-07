using FluentValidation.Results;
using Pot.App.Concerns.Validation;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

internal static class ValidationResultExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var errorDetails = validationResult.ToProblemDetailsErrors();

        return ApiProblemDetailsFactory.CreateUnprocessableEntity(errorDetails);
    }
}

