namespace Pot.App.Errors;

public class ApiDetailErrorCollection : ApiErrorBase
{
    public ApiDetailError[] Errors { get; init; }

    public ApiDetailErrorCollection(ErrorType errorType, IEnumerable<ApiDetailError> errors)
        : base(errorType)
    {
        Errors = [.. errors];
    }
}