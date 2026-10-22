using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

/// <summary>
/// Completes every problem body with the members the contract requires, whatever produced it.
/// </summary>
/// <remarks>
/// The framework's problem-details service runs this for every route into an error body: the exception handlers, the
/// status code pages, the rate limiter, the framework's own not-found and method-not-allowed responses, and a handler
/// that returns a problem result. Leaving these members to each producer makes them drift one producer at a time.
/// </remarks>
internal sealed class ProblemDetailsOptionsSetup : IConfigureOptions<ProblemDetailsOptions>
{
    /// <summary>The Problem Details member carrying the support id. The schema transformer documents it by this name.</summary>
    internal const string CorrelationIdExtension = "correlationId";

    private const string FallbackDetail = "The request could not be completed.";

    // The message a status states when the failure supplies none of its own. A 400 is absent on purpose: its
    // messages are the field errors a validation problem carries instead.
    private static readonly Dictionary<int, string> DefaultDetails = new()
    {
        [StatusCodes.Status401Unauthorized] = "Authentication is required.",
        [StatusCodes.Status403Forbidden] = "You do not have permission to perform this action.",
        [StatusCodes.Status404NotFound] = "The requested resource was not found.",
        [StatusCodes.Status405MethodNotAllowed] = "The requested method is not allowed for this resource.",
        [StatusCodes.Status408RequestTimeout] = "The request timed out.",
        [StatusCodes.Status413PayloadTooLarge] = "The request body is too large.",
        [StatusCodes.Status415UnsupportedMediaType] = "The request content type is not supported.",
        [StatusCodes.Status429TooManyRequests] = "Too many requests. Please wait and try again.",
        [StatusCodes.Status500InternalServerError] = "An unexpected error occurred."
    };

    /// <summary>
    /// Adds the common members to a problem body as it is written.
    /// </summary>
    /// <param name="options">The problem details options to configure.</param>
    public void Configure(ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var problemDetails = context.ProblemDetails;

            // The indexer, not Extensions.Add: this runs while an error response is being written,
            // and Add throws on a key that is already present.
            problemDetails.Extensions[CorrelationIdExtension] = context.HttpContext.TraceIdentifier;

            // Written as the standard member, never as an extension, so the body cannot carry two 'instance' keys.
            problemDetails.Instance = context.HttpContext.Request.Path.ToString();

            if (string.IsNullOrWhiteSpace(problemDetails.Detail))
            {
                problemDetails.Detail = GetDefaultDetail(problemDetails.Status);
            }
        };
    }

    private static string? GetDefaultDetail(int? statusCode)
    {
        if (statusCode is null || statusCode == StatusCodes.Status400BadRequest)
        {
            return null;
        }

        return DefaultDetails.TryGetValue(statusCode.Value, out var detail)
            ? detail
            : FallbackDetail;
    }
}
