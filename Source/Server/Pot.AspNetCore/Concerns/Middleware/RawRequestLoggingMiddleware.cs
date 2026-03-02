using System.Text;

namespace Pot.AspNetCore.Concerns.Middleware;

/// <summary>
/// Logs the raw inbound request payload before endpoint handlers execute.
/// </summary>
/// <remarks>
/// This middleware is intended for request-deserialization troubleshooting in non-production environments.
/// It enables request buffering, reads the full body as text for logging, and then rewinds the stream so
/// downstream components (including endpoint binding and JSON deserializers) can read the body normally.
/// </remarks>
internal sealed class RawRequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RawRequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawRequestLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">Logger used to emit structured request-body diagnostics.</param>
    public RawRequestLoggingMiddleware(ILogger<RawRequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reads and logs the inbound request body, then passes control to the next middleware.
    /// </summary>
    /// <remarks>
    /// If the request body is unreadable or explicitly empty (<c>Content-Length: 0</c>), this middleware
    /// skips logging and immediately invokes the next delegate.
    ///
    /// For readable bodies, this middleware:
    /// <list type="number">
    /// <item><description>Enables buffering so the body stream can be read multiple times.</description></item>
    /// <item><description>Resolves request text encoding from the <c>charset</c> parameter in <c>Content-Type</c> (defaulting to UTF-8).</description></item>
    /// <item><description>Reads the complete payload text for logging.</description></item>
    /// <item><description>Resets the stream position to <c>0</c> so downstream model binding works as expected.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="httpContext">The current HTTP request/response context.</param>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var request = httpContext.Request;

        if (!request.Body.CanRead || request.ContentLength is 0)
        {
            await next(httpContext);
            return;
        }

        request.EnableBuffering();

        string rawBody;
        var encoding = ResolveEncoding(request.ContentType) ?? Encoding.UTF8;

        using (var reader = new StreamReader(request.Body, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        request.Body.Position = 0;

        _logger.LogInformation(
            "Inbound request body for {Method} {Path}: {RawBody}",
            request.Method,
            request.Path,
            rawBody);

        await next(httpContext);
    }

    /// <summary>
    /// Resolves text encoding from an HTTP <c>Content-Type</c> header value.
    /// </summary>
    /// <param name="contentType">The raw <c>Content-Type</c> header value.</param>
    /// <returns>
    /// The resolved <see cref="Encoding"/> when a valid <c>charset</c> parameter is present;
    /// otherwise <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This method tolerates missing or invalid charset values and returns <see langword="null"/> in those cases,
    /// allowing callers to apply a safe default.
    /// </remarks>
    private static Encoding? ResolveEncoding(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        const string charsetPrefix = "charset=";
        var charsetIndex = contentType.IndexOf(charsetPrefix, StringComparison.OrdinalIgnoreCase);

        if (charsetIndex < 0)
        {
            return null;
        }

        var charset = contentType[(charsetIndex + charsetPrefix.Length)..].Trim();

        var separatorIndex = charset.IndexOf(';');
        if (separatorIndex >= 0)
        {
            charset = charset[..separatorIndex].Trim();
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
