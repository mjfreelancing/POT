namespace Pot.AspNetCore.ProblemDetails;

internal sealed class EntityProblemDetails : ProblemDetailsExtensionBase
{
    public required string EntityType { get; init; }
}
