using Pot.App.Errors;

namespace Pot.App.Extensions;

public static class ProblemDetailsErrorExtensions
{
    public static object GetErrorDetails(this ProblemDetailsError error)
    {
        // See ValidationFailureExtensions.AddCustomState() for how custom state is added
        if (error.CustomState is not IDictionary<string, object?> values)
        {
            return new
            {
                error.ErrorCode,
                error.PropertyName,
                error.AttemptedValue,
                error.ErrorMessage
            };
        }

        return new Dictionary<string, object?>(values)
        {
            { "ErrorCode", error.ErrorCode },
            { "PropertyName", error.PropertyName },
            { "AttemptedValue", error.AttemptedValue },
            { "ErrorMessage", error.ErrorMessage },
        };
    }
}
