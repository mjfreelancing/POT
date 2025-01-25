using Pot.AspNetCore.Concerns.Validation;
using Pot.Data;
using Pot.Data.Entities;
using System.Net;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsFactory
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity(string propertyName, object? attemptedValue,
        string errorMessage, string? errorCode = default)
    {
        var errorDetails = new ProblemDetailsError
        {
            PropertyName = propertyName,
            ErrorCode = errorCode,
            AttemptedValue = attemptedValue,
            ErrorMessage = errorMessage
        };

        return CreateUnprocessableEntity(errorDetails);

    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity(params IEnumerable<ProblemDetailsError> errorDetails)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.UnprocessableEntity,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errorDetails.ToArray() }
            }
        };
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateETagConflict(EntityBase entity, object? attemptedValue)
    {
        var errorDetails = new ProblemDetailsError[]
        {
            new()
            {
                PropertyName = nameof(EntityBase.Etag),
                ErrorCode = ErrorCodes.Conflict,
                AttemptedValue = attemptedValue,
                ErrorMessage = $"The {DbContextBase.GetTableNameFromEntity(entity)} entity tag does not match the current record."
            }
        };

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = "Unable to save changes.",
            Status = (int)HttpStatusCode.Conflict,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errorDetails }
            }
        };
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateEntityExistsConflict(EntityBase entity, string propertyName, string attemptedValue)
    {
        var errorDetails = new ProblemDetailsError[]
        {
            new()
            {
                PropertyName = propertyName,
                ErrorCode = ErrorCodes.Conflict,
                AttemptedValue = attemptedValue,
                ErrorMessage = $"The update would conflict with another {DbContextBase.GetTableNameFromEntity(entity)} entity."
            }
        };

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = "Unable to save changes.",
            Status = (int)HttpStatusCode.Conflict,
            Extensions = new Dictionary<string, object?>
            {
                { "errors", errorDetails }
            }
        };
    }
}