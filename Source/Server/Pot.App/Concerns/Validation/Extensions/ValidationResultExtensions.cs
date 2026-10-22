using FluentValidation.Results;
using Pot.App.Errors;

namespace Pot.App.Concerns.Validation.Extensions;

public static class ValidationResultExtensions
{
    public static IEnumerable<ApiDetailError> ToApiDetailErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors.Select(error => new ApiDetailError(ErrorType.BadRequest)
        {
            ErrorCode = error.ErrorCode,
            PropertyName = error.PropertyName,
            ErrorMessage = error.ErrorMessage,
            CustomState = error.CustomState
        });
    }
}
