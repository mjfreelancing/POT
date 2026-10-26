using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Pot.AspNetCore.Concerns.OpenApi;

// Explains the categories a conflict can belong to, on every operation that declares one.
//
// The category travels in 'type', so a caller never has to infer it from a field name, and a generated client cannot
// tell from a status code alone which failures mean "refresh and retry". The endpoint says whether its rule can refuse
// a stale entity tag; the wording is written once here rather than repeated on every route.
internal sealed class ConflictResponseOperationTransformer : IOpenApiOperationTransformer
{
    private const string ConflictStatusCode = "409";

    private const string RefusedCategories =
        "A conflict names its category in `type`: `urn:pot:problem:duplicate-value` means the value already exists, " +
        "and `urn:pot:problem:data-conflict` means the database rejected a constraint or a race.";

    private const string StaleUpdateDescription =
        "A conflict names its category in `type`: `urn:pot:problem:stale-entity-tag` means the record changed since " +
        "it was read, so refresh and retry; `urn:pot:problem:duplicate-value` means the value already exists; and " +
        "`urn:pot:problem:data-conflict` means the database rejected a constraint or a race.";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation.Responses is null || !operation.Responses.TryGetValue(ConflictStatusCode, out var conflictResponse))
        {
            return Task.CompletedTask;
        }

        conflictResponse.Description = RefusesAStaleEntityTag(context)
            ? StaleUpdateDescription
            : RefusedCategories;

        return Task.CompletedTask;
    }

    private static bool RefusesAStaleEntityTag(OpenApiOperationTransformerContext context)
    {
        return context.Description.ActionDescriptor.EndpointMetadata.OfType<StaleEntityTagConflictMetadata>().Any();
    }
}
