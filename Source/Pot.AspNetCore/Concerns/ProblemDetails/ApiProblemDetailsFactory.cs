using Pot.AspNetCore.Concerns.Validation;
using Pot.Data;
using Pot.Data.Entities;
using System.Net;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

// ProblemDetailsFactory already exists in Microsoft.AspNetCore.Mvc.Infrastructure
internal static class ApiProblemDetailsFactory
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity(string message)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = message,
            Status = (int)HttpStatusCode.UnprocessableEntity
        };
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity(string errorCode, string propertyName,
        object? attemptedValue, string errorMessage)
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

    // Note: If errorDetails is passed as IEnumerable<ProblemDetailsError> then the serialization of "errors" will only contain the properties
    // of that type. If a derived class is passed in then the additional properties will not be serialized. I can only assume this is due to
    // the serialization process only looks at the reflected type, which would be ProblemDetailsError[].
    // Two options:
    // 1. Use  { "errors", errorDetails.Cast<object>().ToArray() }
    // 2. Use generics (as used)
    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity<TErrorType>(params IEnumerable<TErrorType> errorDetails) where TErrorType : ProblemDetailsError
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
                ErrorMessage = $"The operation would conflict with another {DbContextBase.GetTableNameFromEntity(entity)} entity."
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