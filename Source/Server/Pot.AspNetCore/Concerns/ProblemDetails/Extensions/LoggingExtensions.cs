namespace Pot.AspNetCore.Concerns.ProblemDetails.Extensions;

internal static class LoggingExtensions
{
    public static void LogErrors(this ILogger logger, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        if (problemDetails.Extensions.TryGetValue("errors", out var errors) && errors is ProblemDetailsError[] problemDetailsErrors)
        {
            var errorMessages = problemDetailsErrors.Select(error => error.ErrorMessage);

            logger.LogInformation("Validation Error(s): {ValidationErrorMessages}", string.Join(", ", errorMessages));
        }
    }
}
