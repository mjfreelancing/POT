using Pot.App.Errors;
using Pot.App.Extensions;

namespace Pot.AspNetCore.Extensions;

internal static class LoggingExtensions
{
    public static void LogErrors(this ILogger logger, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        if (problemDetails.Extensions.TryGetValue("errors", out var errors) && errors is ProblemDetailsError[] problemDetailsErrors)
        {
            foreach (var error in problemDetailsErrors)
            {
                logger.LogError(error);
            }
        }
    }
}