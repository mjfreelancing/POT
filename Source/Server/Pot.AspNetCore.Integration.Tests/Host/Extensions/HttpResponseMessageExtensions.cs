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

    public static void ShouldNotContainHeader(this HttpResponseMessage response, string headerName, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(headerName);

        response.Headers.Contains(headerName).ShouldBeFalse(customMessage);
    }
}