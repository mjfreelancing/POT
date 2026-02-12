namespace Pot.App.Errors;

public class ApiDetailError : ApiBasicError
{
    public string PropertyName { get; init; } = string.Empty;
    public object? AttemptedValue { get; init; }

    public ApiDetailError(ErrorType errorType)
        : base(errorType)
    {
    }
}
