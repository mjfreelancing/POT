namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal abstract class ProblemDetailsExtensionBase
{
    public required string ErrorMessage { get; init; }
}
