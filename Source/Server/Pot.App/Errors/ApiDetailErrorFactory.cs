using Pot.Data.Entities;

namespace Pot.App.Errors;

public static class ApiDetailErrorFactory
{
    public static ApiDetailError CreateEntityExistsError(string propertyName, string errorMessage)
    {
        return new ApiDetailError(ErrorType.Conflict)
        {
            ErrorCode = ErrorCodes.Duplicate,
            PropertyName = propertyName,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateEntityNotFoundError(string errorMessage)
    {
        return new ApiDetailError(ErrorType.NotFound)
        {
            ErrorCode = ErrorCodes.NotFound,
            PropertyName = string.Empty,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateEntityConstraintError(string propertyName, string errorMessage)
    {
        return new ApiDetailError(ErrorType.Constraint)
        {
            ErrorCode = ErrorCodes.Constraint,
            PropertyName = propertyName,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateAuthError(string errorMessage)
    {
        return new ApiDetailError(ErrorType.Auth)
        {
            ErrorCode = ErrorCodes.Auth,
            PropertyName = string.Empty,
            ErrorMessage = errorMessage
        };
    }

    public static ApiBasicError CreateBadRequestError(string errorMessage)
    {
        return new ApiBasicError(ErrorType.BadRequest)
        {
            ErrorCode = ErrorCodes.Invalid,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateBadRequestError(string propertyName, string errorMessage)
    {
        return new ApiDetailError(ErrorType.BadRequest)
        {
            ErrorCode = ErrorCodes.Invalid,
            PropertyName = propertyName,
            ErrorMessage = errorMessage
        };
    }

    public static ApiDetailError CreateEtagConflict(string entityType)
    {
        return new ApiDetailError(ErrorType.Conflict)
        {
            ErrorCode = ErrorCodes.StaleEntityTag,
            PropertyName = nameof(EntityBase.Etag),
            ErrorMessage = $"The entity tag for the {entityType} does not match the current record"
        };
    }

    public static ApiDetailError CreateTooManyRequests(int? retryAfterSeconds)
    {
        // RFC 9110 defines Retry-After as an integer delta-seconds value (or an HTTP date). The wait is
        // unknown when the rate-limit lease carries no RetryAfter metadata; saying "after 0 seconds"
        // would invite a retry that the limiter rejects again.
        var errorMessage = retryAfterSeconds is null
            ? "Too many requests. Please wait and try again."
            : $"Too many requests. Please wait and try again after {retryAfterSeconds} seconds.";

        return new ApiDetailError(ErrorType.TooManyRequests)
        {
            ErrorCode = ErrorCodes.TooManyRequests,
            ErrorMessage = errorMessage
        };
    }
}
