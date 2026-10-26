namespace Pot.AspNetCore.Concerns.OpenApi;

/// <summary>
/// Marks an endpoint whose conflict rule can refuse a stale entity tag.
/// </summary>
/// <remarks>
/// Document generation reads it to describe the conflict categories the operation can produce. No runtime behaviour
/// depends on it: whether a request is refused depends on the entity tag, not on this marker.
/// </remarks>
internal sealed class StaleEntityTagConflictMetadata
{
}
