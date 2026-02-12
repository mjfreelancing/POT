using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Extensions;

public static class LoggerExtensions
{
    public static void LogApiError(this ILogger logger, ApiDetailError apiDetailError)
    {
        logger.LogInformation("Validation Error: {ValidationErrorMessage} (Property: {Property}, Value: {Value})",
            apiDetailError.ErrorMessage,
            apiDetailError.PropertyName ?? string.Empty,
            apiDetailError.AttemptedValue ?? string.Empty);
    }
}