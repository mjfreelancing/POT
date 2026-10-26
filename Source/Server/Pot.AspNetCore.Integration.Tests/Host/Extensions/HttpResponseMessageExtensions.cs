using AllOverIt.Assertion;
using Pot.AspNetCore.Integration.Tests.Host.Models;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

public static class HttpResponseMessageExtensions
{
    private const string ProblemJsonContentType = "application/problem+json";
    private const string ValidationProblemTitle = "One or more validation errors occurred.";

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
    /// Asserts the response is a problem body for the given status, and returns it.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedStatus">The status the failure must answer with.</param>
    /// <param name="expectedType">The problem type the body must declare, or <see langword="null" /> to leave the type unasserted.</param>
    /// <param name="expectedDetail">The message the body must state, or <see langword="null" /> to only require one.</param>
    /// <param name="customMessage">An optional message explaining what the failure means for the test.</param>
    /// <returns>The parsed problem response, so a caller can assert anything else it reports.</returns>
    /// <remarks>
    /// A failure other than a 400 states what happened in <c>detail</c> alone, so its <c>errors</c> member must be
    /// absent: a client reads one message for every status, and a second place to look is a second shape.
    /// </remarks>
    public static async Task<ProblemResponse> ShouldBeProblemAsync(this HttpResponseMessage response,
        HttpStatusCode expectedStatus, string? expectedType = null, string? expectedDetail = null, string? customMessage = null)
    {
        _ = response.WhenNotNull();

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var message = customMessage is null ? body : $"{customMessage}{Environment.NewLine}{body}";

        response.StatusCode.ShouldBe(expectedStatus, message);

        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJsonContentType, message);

        var problem = JsonSerializer.Deserialize<ProblemResponse>(body, JsonSerializerOptions.Web);

        problem.ShouldNotBeNull(message);

        if (expectedType is not null)
        {
            problem!.Type.ShouldBe(expectedType, message);
        }

        problem!.Errors.ShouldBeEmpty($"a {expectedStatus} carries its message in detail only{Environment.NewLine}{message}");

        if (expectedDetail is null)
        {
            problem.Detail.ShouldNotBeNullOrWhiteSpace(message);
        }
        else
        {
            problem.Detail.ShouldBe(expectedDetail, message);
        }

        return problem;
    }

    /// <summary>
    /// Asserts the response is a validation problem and returns it.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedFieldKeys">The fields the failure must name, as the caller reads them.</param>
    /// <returns>The parsed problem response, so a caller can assert the messages it reports.</returns>
    /// <remarks>
    /// A 400 states its messages in the <c>errors</c> map and carries no <c>detail</c>, and no implementation detail
    /// reaches the body: no validator class name and no submitted value.
    /// </remarks>
    public static async Task<ProblemResponse> ShouldBeValidationProblemAsync(this HttpResponseMessage response,
        params string[] expectedFieldKeys)
    {
        _ = response.WhenNotNull();

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJsonContentType, body);

        body.ShouldNotContain("errorCode", Case.Sensitive);
        body.ShouldNotContain("attemptedValue", Case.Sensitive);

        var problem = JsonSerializer.Deserialize<ProblemResponse>(body, JsonSerializerOptions.Web);

        problem.ShouldNotBeNull(body);

        problem!.Title.ShouldBe(ValidationProblemTitle, body);
        problem.Detail.ShouldBeNull($"a 400 states its messages in the map{Environment.NewLine}{body}");
        problem.Errors.ShouldNotBeEmpty(body);

        foreach (var expectedFieldKey in expectedFieldKeys)
        {
            problem.Errors.ShouldContainKey(expectedFieldKey, body);
            problem.Errors[expectedFieldKey].ShouldNotBeEmpty(body);
        }

        return problem;
    }

    /// <summary>
    /// Asserts the response is the conflict a uniqueness rule produces.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="customMessage">An optional message explaining what the conflict means for the test.</param>
    /// <returns>The parsed problem response, so a caller can assert the message it reports.</returns>
    /// <remarks>
    /// Asserting the status alone would not show that a rule rejected the request. A duplicate that is not caught
    /// beforehand reaches the database, and the unique constraint that follows is answered with a 409 as well - but
    /// with the data-conflict type. Checking the type is what makes the test fail if the rule stops running, instead
    /// of passing on the database's answer.
    /// </remarks>
    public static async Task<ProblemResponse> ShouldBeEntityConflictAsync(this HttpResponseMessage response,
        string? customMessage = null)
    {
        return await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ProblemTypes.DuplicateValue,
            customMessage: customMessage);
    }

    /// <summary>
    /// Asserts the response is the conflict a stale entity tag produces.
    /// </summary>
    /// <param name="response">The response to assert.</param>
    /// <param name="expectedDetail">The message the body must state about the entity it refused.</param>
    /// <param name="customMessage">An optional message explaining what the conflict means for the test.</param>
    /// <returns>The parsed problem response.</returns>
    public static async Task<ProblemResponse> ShouldBeStaleEtagConflictAsync(this HttpResponseMessage response,
        string expectedDetail, string? customMessage = null)
    {
        return await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, ProblemTypes.StaleEntityTag,
            expectedDetail, customMessage);
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