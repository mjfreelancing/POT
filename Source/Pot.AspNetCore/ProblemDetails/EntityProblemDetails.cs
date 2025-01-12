namespace Pot.AspNetCore.ProblemDetails;

internal class EntityProblemDetails : ProblemDetailsExtension
{
    public required string EntityType { get; init; }
}
