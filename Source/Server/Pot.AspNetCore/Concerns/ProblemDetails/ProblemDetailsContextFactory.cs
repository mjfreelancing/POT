namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    public static ProblemDetailsContext Create<TProblemDetails>(HttpContext httpContext, Exception exception, int statusCode, TProblemDetails[]? errorDetails = default)
        where TProblemDetails : ProblemDetailsExtensionBase
    {
        httpContext.Response.StatusCode = statusCode;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Detail = exception.Message,
                Status = httpContext.Response.StatusCode
            },
            Exception = exception
        };

        if (errorDetails is not null)
        {
            context.ProblemDetails.Extensions.Add("errors", errorDetails);
        }

        return context;
    }
}
