using AllOverIt.Extensions;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal static class ProblemDetailsContextFactory
{
    /// <summary>
    /// Creates the context a problem body is written from.
    /// </summary>
    /// <param name="httpContext">The request being answered.</param>
    /// <param name="statusCode">The status the failure answers with.</param>
    /// <param name="detail">The message that states what happened, when the failure has one of its own.</param>
    /// <param name="exception">The exception the failure came from, when there was one.</param>
    /// <param name="type">The problem type the failure belongs to, when it declares one.</param>
    /// <returns>The context to write.</returns>
    /// <remarks>
    /// Only the members this failure knows are set. The shared customisation adds the correlation id, the instance and
    /// the default detail, and no failure other than a 400 carries field errors.
    /// </remarks>
    public static ProblemDetailsContext Create(HttpContext httpContext, int statusCode, string? detail,
        Exception? exception, string? type = null)
    {
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Detail = detail,
            Status = statusCode
        };

        if (type is not null)
        {
            problemDetails.Type = type;
        }

        return new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        };
    }
}
