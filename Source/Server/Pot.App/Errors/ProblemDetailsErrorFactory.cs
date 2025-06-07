using Pot.Data.Entities;

namespace Pot.App.Errors;

public static class ProblemDetailsErrorFactory
{
    public static ProblemDetailsError CreateEntityExistsError(string entityType, string propertyName, object? attemptedValue)
    {
        return new ProblemDetailsError(ProblemType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = $"The operation would conflict with another {entityType} entity."
        };
    }

    public static ProblemDetailsError CreateEntityNotFoundError(string entityType, string propertyName, object? attemptedValue)
    {
        return new ProblemDetailsError(ProblemType.NotFound)
        {
            ErrorCode = ErrorCodes.NotFound,
            PropertyName = propertyName,
            AttemptedValue = attemptedValue,
            ErrorMessage = $"The {entityType} does not exist."
        };
    }

    public static ProblemDetailsError CreateEtagConflict(string entityType, object? attemptedValue)
    {
        return new ProblemDetailsError(ProblemType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = nameof(EntityBase.Etag),
            AttemptedValue = attemptedValue,
            ErrorMessage = $"The entity tag for the {entityType} does not match the current record."
        };
    }
}
