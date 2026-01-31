using Pot.Data.Entities;

namespace Pot.App.Errors;

public static class ProblemDetailsErrorFactory
{
    public static ProblemDetailsError CreateEntityExistsError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ProblemDetailsError(ProblemType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage//$"The operation would conflict with another {entityType} entity."
        };
    }

    public static ProblemDetailsError CreateEntityNotFoundError(object? attemptedValue, string errorMessage)
    {
        return new ProblemDetailsError(ProblemType.NotFound)
        {
            ErrorCode = ErrorCodes.NotFound,
            PropertyName = string.Empty,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ProblemDetailsError CreateEntityConstraintError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ProblemDetailsError(ProblemType.Constraint)
        {
            ErrorCode = ErrorCodes.Constraint,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ProblemDetailsError CreateAuthError(string errorMessage)
    {
        return new ProblemDetailsError(ProblemType.Auth)
        {
            ErrorCode = ErrorCodes.Auth,
            PropertyName = string.Empty,
            AttemptedValue = string.Empty,
            ErrorMessage = errorMessage
        };
    }

    public static ProblemDetailsBasicError CreateUnprocessableEntityError(string errorMessage)
    {
        return new ProblemDetailsBasicError(ProblemType.UnprocessableEntity)
        {
            ErrorCode = ErrorCodes.Invalid,
            ErrorMessage = errorMessage
        };
    }

    public static ProblemDetailsError CreateUnprocessableEntityError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ProblemDetailsError(ProblemType.UnprocessableEntity)
        {
            ErrorCode = ErrorCodes.Invalid,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ProblemDetailsError CreateEtagConflict(string entityType, object? attemptedValue)
    {
        return new ProblemDetailsError(ProblemType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = nameof(EntityBase.Etag),
            AttemptedValue = attemptedValue,
            ErrorMessage = $"The entity tag for the {entityType} does not match the current record"
        };
    }

    public static ProblemDetailsError CreateTooManyRequests(double totalSeconds)
    {
        return new ProblemDetailsError(ProblemType.TooManyRequests)
        {
            ErrorCode = ErrorCodes.TooManyRequests,
            ErrorMessage = $"Too many requests. Please wait and try again after {totalSeconds} seconds."
        };
    }
}
