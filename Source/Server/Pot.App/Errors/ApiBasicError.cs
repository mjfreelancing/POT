namespace Pot.App.Errors;

public class ApiBasicError : ApiErrorBase
{
    // These properties are specific error details
    public required string ErrorCode { get; init; }
    public required string ErrorMessage { get; init; }
    public object? CustomState { get; init; } = null;

    public ApiBasicError(ErrorType errorType)
        : base(errorType)
    {
    }
}
