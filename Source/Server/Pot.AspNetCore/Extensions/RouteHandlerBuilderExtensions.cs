using Pot.AspNetCore.Concerns.OpenApi;

namespace Pot.AspNetCore.Extensions;

internal static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds the responses a request body can be refused with before the handler sees it.
    /// </summary>
    /// <param name="builder">The route handler builder to configure.</param>
    /// <returns>The configured route handler builder.</returns>
    /// <remarks>
    /// Only an endpoint that reads a body can be refused for its size or for taking too long to arrive, so this is
    /// declared per route rather than on the group.
    /// </remarks>
    public static RouteHandlerBuilder ProducesRequestBodyProblems(this RouteHandlerBuilder builder)
    {
        return builder
            .ProducesProblem(StatusCodes.Status408RequestTimeout)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);
    }

    /// <summary>
    /// Adds the conflict response of a rule that compares an entity tag, and marks the operation as one that can
    /// refuse a stale update.
    /// </summary>
    /// <param name="builder">The route handler builder to configure.</param>
    /// <returns>The configured route handler builder.</returns>
    public static RouteHandlerBuilder ProducesStaleEntityTagConflict(this RouteHandlerBuilder builder)
    {
        return builder
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithMetadata(new StaleEntityTagConflictMetadata());
    }
}
