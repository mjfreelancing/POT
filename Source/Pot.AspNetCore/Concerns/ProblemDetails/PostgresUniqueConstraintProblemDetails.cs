namespace Pot.AspNetCore.Concerns.ProblemDetails;

internal sealed class PostgresUniqueConstraintProblemDetails : ProblemDetailsExtensionBase
{
    public required IDictionary<string, object?> Properties { get; init; }
}
