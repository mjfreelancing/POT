namespace Pot.AspNetCore.Features.Settings.Update;

// This is needed to pass the category and key from the endpoint to the handler, since they are not
// part of the request body and we want to avoid using route parameters in the handler signature.
internal sealed class RequestValidationContext
{
    public required string Category { get; init; }
    public required string Key { get; init; }
}
