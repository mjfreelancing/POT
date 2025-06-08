using AllOverIt.Extensions;
using Pot.App.Errors;
using Pot.App.Extensions;
using System.Net;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

// ProblemDetailsFactory already exists in Microsoft.AspNetCore.Mvc.Infrastructure
internal static class ApiProblemDetailsFactory
{
    // **************************************************************************
    // TODO: *** ALL TO BE REMOVED WHEN TH IMPORT IMPLEMENTATION IS RE-WORKED ***
    // **************************************************************************


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
                { "errors", errorDetails.SelectToArray(error => error.GetErrorDetails()) }
            }
        };
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails CreateUnprocessableEntity(string message)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = message,
            Status = (int)HttpStatusCode.UnprocessableEntity
        };
    }
}