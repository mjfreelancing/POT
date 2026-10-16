namespace Pot.AspNetCore.Extensions;

internal static class RouteGroupBuilderExtensions
{
    /// <summary>
    /// Adds the shared bad-request and server-error response metadata to a route group.
    /// </summary>
    /// <param name="builder">The route group builder to configure.</param>
    /// <returns>The configured route group builder.</returns>
    public static RouteGroupBuilder ProducesStandardProblems(this RouteGroupBuilder builder)
    {
        return builder
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
