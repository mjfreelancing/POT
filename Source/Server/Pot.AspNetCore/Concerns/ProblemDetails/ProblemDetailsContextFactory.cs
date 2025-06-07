using Pot.App.Errors;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    public static ProblemDetailsContext Create(HttpContext httpContext, int statusCode, string detail, Exception exception, ProblemDetailsError[] errorDetails)
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

        context.ProblemDetails.Extensions.Add("errors", errorDetails);

        return context;
    }
}
