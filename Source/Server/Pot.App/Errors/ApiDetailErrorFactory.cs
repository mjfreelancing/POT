using Pot.Data.Entities;

namespace Pot.App.Errors;

public static class ApiDetailErrorFactory
{
    public static ApiDetailError CreateEntityExistsError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ApiDetailError(ErrorType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage//$"The operation would conflict with another {entityType} entity."
        };
    }

    public static ApiDetailError CreateEntityNotFoundError(object? attemptedValue, string errorMessage)
    {
        return new ApiDetailError(ErrorType.NotFound)
        {
            ErrorCode = ErrorCodes.NotFound,
            PropertyName = string.Empty,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateEntityConstraintError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ApiDetailError(ErrorType.Constraint)
        {
            ErrorCode = ErrorCodes.Constraint,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateAuthError(string errorMessage)
    {
        return new ApiDetailError(ErrorType.Auth)
        {
            ErrorCode = ErrorCodes.Auth,
            PropertyName = string.Empty,
            AttemptedValue = string.Empty,
            ErrorMessage = errorMessage
        };
    }

    public static ApiBasicError CreateUnprocessableEntityError(string errorMessage)
    {
        return new ApiBasicError(ErrorType.UnprocessableEntity)
        {
            ErrorCode = ErrorCodes.Invalid,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateUnprocessableEntityError(string propertyName, object? attemptedValue, string errorMessage)
    {
        return new ApiDetailError(ErrorType.UnprocessableEntity)
        {
            ErrorCode = ErrorCodes.Invalid,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateEtagConflict(string entityType, object? attemptedValue)
    {
        return new ApiDetailError(ErrorType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = nameof(EntityBase.Etag),
            AttemptedValue = attemptedValue,
            ErrorMessage = $"The entity tag for the {entityType} does not match the current record"
        };
    }

    public static ApiDetailError CreateTooManyRequests(double totalSeconds)
    {
        return new ApiDetailError(ErrorType.TooManyRequests)
        {
            ErrorCode = ErrorCodes.TooManyRequests,
            ErrorMessage = $"Too many requests. Please wait and try again after {totalSeconds} seconds."
        };
    }
}
