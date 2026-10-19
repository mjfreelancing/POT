using AllOverIt.Assertion;
using AllOverIt.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Pot.AspNetCore.Extensions;

internal static class HttpRequestExtensions
{
    // An empty or whitespace header is treated as absent, so the server-generated identifier is used instead.
    public static bool TryGetCorrelationId(this HttpRequest request, [NotNullWhen(true)] out string? correlationId)
    {
        _ = request.WhenNotNull();

        if (request.Headers.TryGetValue("X-Correlation-Id", out var values) && !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            correlationId = values.First()!;
            return true;
        }

        correlationId = null;
        return false;
    }
}
