namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal sealed class EntityProblemDetails : ProblemDetailsExtensionBase
{
    public required string EntityType { get; init; }
}
