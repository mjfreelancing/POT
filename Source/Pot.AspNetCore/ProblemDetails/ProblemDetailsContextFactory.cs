namespace Pot.AspNetCore.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    public static ProblemDetailsContext Create(HttpContext httpContext, Exception exception, int statusCode, ProblemDetailsExtensionBase[]? errorDetails = default)
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
