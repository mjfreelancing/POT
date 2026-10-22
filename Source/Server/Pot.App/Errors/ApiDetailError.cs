namespace Pot.App.Errors;

public class ApiDetailError : ApiBasicError
{
    public string PropertyName { get; init; } = string.Empty;

    public ApiDetailError(ErrorType errorType)
        : base(errorType)
    {
    }
}
