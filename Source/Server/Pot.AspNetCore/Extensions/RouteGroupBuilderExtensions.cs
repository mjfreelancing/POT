namespace Pot.AspNetCore.Extensions;

internal static class RouteGroupBuilderExtensions
{
    /// <summary>
    /// Adds the shared bad-request and server-error response metadata to a route group.
    /// </summary>
    /// <param name="builder">The route group builder to configure.</param>
    /// <returns>The configured route group builder.</returns>
    /// <remarks>
    /// The bad request is declared as the validation problem, because every 400 answers with the map of field to
    /// messages rather than with any other problem body.
    /// </remarks>
    public static RouteGroupBuilder ProducesStandardProblems(this RouteGroupBuilder builder)
    {
        return builder
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
