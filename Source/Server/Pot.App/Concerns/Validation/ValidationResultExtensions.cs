using FluentValidation.Results;
using Pot.App.Errors;

namespace Pot.App.Concerns.Validation;

public static class ValidationResultExtensions
{
    public static IEnumerable<ProblemDetailsError> ToProblemDetailsErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors.Select(error => new ProblemDetailsError(ProblemType.UnprocessableEntity)
        {
            ErrorCode = error.ErrorCode,
            PropertyName = error.PropertyName,
            AttemptedValue = error.AttemptedValue,
            ErrorMessage = error.ErrorMessage,
            CustomState = error.CustomState
        });
    }
}
