using FluentValidation.Results;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

internal static class ValidationResultExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var errorDetails = validationResult.ToProblemDetailsErrors();

        return ApiProblemDetailsFactory.CreateUnprocessableEntity(errorDetails);
    }

    public static IEnumerable<ProblemDetailsError> ToProblemDetailsErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors.Select(error => new ProblemDetailsError
        {
            PropertyName = error.PropertyName,
            ErrorCode = error.ErrorCode,
            AttemptedValue = error.AttemptedValue,
            ErrorMessage = error.ErrorMessage
        });
    }
}

