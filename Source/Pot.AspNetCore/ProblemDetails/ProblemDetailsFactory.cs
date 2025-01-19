using System.Net;

namespace Pot.AspNetCore.ProblemDetails;

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
}