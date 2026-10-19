namespace Pot.AspNetCore.Extensions;

internal static class LoggingExtensions
{
    private const string ErrorsKey = "errors";
    private const string ErrorCodeKey = "errorCode";
    private const string ErrorMessageKey = "errorMessage";
    private const string PropertyNameKey = "propertyName";

    public static void LogErrors(this ILogger logger, Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        if (!problemDetails.Extensions.TryGetValue(ErrorsKey, out var errors) || errors is not IEnumerable<object> errorDetails)
        {
            return;
        }

        // AttemptedValue is deliberately not logged: for login, signup and password reset it can be a
        // password or an OTP.
        foreach (var errorDetail in errorDetails.OfType<IDictionary<string, object?>>())
        {
            logger.LogInformation(
                "Validation Error: {ValidationErrorMessage} (Code: {ValidationErrorCode}, Property: {Property})",
                GetValue(errorDetail, ErrorMessageKey),
                GetValue(errorDetail, ErrorCodeKey),
                GetValue(errorDetail, PropertyNameKey));
        }
    }

    private static string GetValue(IDictionary<string, object?> details, string key)
    {
        return details.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }
}