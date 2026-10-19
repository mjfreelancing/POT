using AllOverIt.Extensions;
using Pot.App.Errors;
using Pot.App.Extensions;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    public static ProblemDetailsContext Create(HttpContext httpContext, int statusCode, string detail, Exception? exception, ApiDetailError[] errorDetails)
    {
        httpContext.Response.StatusCode = statusCode;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Detail = detail,
                Status = httpContext.Response.StatusCode
            },
            Exception = exception
        };

        context.ProblemDetails.Extensions["errors"] = errorDetails.SelectToArray(error => error.GetErrorDetails());

        return context;
    }
}
