using Pot.App.Errors;
using Pot.App.Extensions;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    public static ProblemDetailsContext Create(HttpContext httpContext, int statusCode, string detail, Exception? exception, ProblemDetailsError[] errorDetails)
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

        var errors = errorDetails.Select(error => error.GetErrorDetails());

        context.ProblemDetails.Extensions.Add("errors", errors);

        return context;
    }
}
