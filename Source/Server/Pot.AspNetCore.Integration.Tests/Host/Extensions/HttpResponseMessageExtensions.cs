using AllOverIt.Assertion;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Asserts the response's status code.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expected">The expected status code.</param>
    /// <param name="customMessage">An optional message explaining what the status means for the test.</param>
    /// <remarks>
    /// The body is read eagerly so that a rejected request explains itself, rather than the failure reporting only
    /// the status code.
    /// </remarks>
    public static async Task ShouldHaveStatusAsync(this HttpResponseMessage response, HttpStatusCode expected,
        string? customMessage = null)
    {
        _ = response.WhenNotNull();

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var message = customMessage is null ? body : $"{customMessage}{Environment.NewLine}{body}";

        response.StatusCode.ShouldBe(expected, message);
    }

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

    /// <summary>
    /// Asserts the response lists a header in <c>Access-Control-Expose-Headers</c>, so that cross-origin script can read it.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="headerName">The header expected to be exposed. Compared case-insensitively.</param>
    public static void ShouldExposeHeader(this HttpResponseMessage response, string headerName)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        var exposedHeaders = response
            .ShouldHaveHeaderValues("Access-Control-Expose-Headers")
            .SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        exposedHeaders.ShouldContain(exposedHeader => string.Equals(exposedHeader, headerName, StringComparison.OrdinalIgnoreCase),
            $"'{headerName}' should be listed in Access-Control-Expose-Headers");
    }

    public static void ShouldNotContainHeader(this HttpResponseMessage response, string headerName, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        response.Headers.Contains(headerName).ShouldBeFalse(customMessage);
    }
}