using FluentValidation.Results;

namespace Pot.App.Concerns.Validation.Extensions;

public static class ValidationFailureExtensions
{
    public static void AddCustomState(this ValidationFailure validationFailure, string propertyName, object? value)
    {
        validationFailure.CustomState ??= new Dictionary<string, object?>();

        var values = validationFailure.CustomState as Dictionary<string, object?>;
        values!.Add(propertyName, value);
    }
}