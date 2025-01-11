namespace Pot.AspNetCore.ProblemDetails;

public class EntityProblemDetails : ProblemDetailsExtension
{
    public required string EntityType { get; init; }
}
