using AllOverIt.Assertion;
using Shouldly;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

public static class HttpResponseMessageExtensions
{
    public static IEnumerable<string> ShouldHaveHeaderValues(this HttpResponseMessage response, string headerName, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        response.Headers.TryGetValues(headerName, out var values).ShouldBeTrue(customMessage);

        return values!;
    }

    public static void ShouldContainHeader(this HttpResponseMessage response, string headerName, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        response.Headers.Contains(headerName).ShouldBeTrue(customMessage);
    }

    public static void ShouldNotContainHeader(this HttpResponseMessage response, string headerName, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        response.Headers.Contains(headerName).ShouldBeFalse(customMessage);
    }
}