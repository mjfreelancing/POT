using AllOverIt.Assertion;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

public static class HttpResponseMessageExtensions
{
    private const string ProblemJsonContentType = "application/problem+json";

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

    /// <summary>
    /// Asserts the response is a conflict that names the field which clashed with an existing record.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedPropertyName">The field the endpoint's rule rejected.</param>
    /// <param name="expectedAttemptedValue">The value that clashed, which the response must echo back.</param>
    /// <param name="customMessage">An optional message explaining what the conflict means for the test.</param>
    /// <returns>The parsed problem response, so a caller can assert the message it reports.</returns>
    /// <remarks>
    /// Asserting the status alone would not show that a rule rejected the request. A duplicate that is not caught
    /// beforehand reaches the database, and the unique constraint that follows is answered with a 409 as well - but
    /// that response names no field and echoes no value. Checking both is what makes the test fail if the rule is
    /// removed, instead of passing on the database's answer.
    /// </remarks>
    public static async Task<ProblemResponse> ShouldBeEntityConflictAsync(this HttpResponseMessage response,
        string expectedPropertyName, string expectedAttemptedValue, string? customMessage = null)
    {
        _ = response.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedPropertyName);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var message = customMessage is null ? body : $"{customMessage}{Environment.NewLine}{body}";

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, message);

        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJsonContentType, message);

        var problem = JsonSerializer.Deserialize<ProblemResponse>(body, JsonSerializerOptions.Web);

        problem.ShouldNotBeNull(message);

        var error = problem!.Errors.ShouldHaveSingleItem(message);

        error.PropertyName.ShouldBe(expectedPropertyName, message);
        error.AttemptedValue?.ToString().ShouldBe(expectedAttemptedValue, message);
        error.ErrorMessage.ShouldNotBeNullOrWhiteSpace(message);

        return problem;
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