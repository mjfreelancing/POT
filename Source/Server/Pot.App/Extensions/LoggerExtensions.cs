using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Extensions;

public static class LoggerExtensions
{
    public static void LogError(this ILogger logger, ProblemDetailsError problemDetailsError)
    {
        logger.LogInformation("Validation Error: {ValidationErrorMessage} (Property: {Property}, Value: {Value})",
            problemDetailsError.ErrorMessage,
            problemDetailsError.PropertyName ?? string.Empty,
            problemDetailsError.AttemptedValue ?? string.Empty);
    }
}