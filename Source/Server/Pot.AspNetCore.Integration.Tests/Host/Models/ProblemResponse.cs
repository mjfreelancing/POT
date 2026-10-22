namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// The error response fields the integration fixtures assert against.
/// </summary>
/// <remarks>
/// Only the asserted fields are declared; the endpoints return more, which deserialization ignores.
/// </remarks>
public sealed class ProblemResponse
{
    /// <summary>
    /// The HTTP status code, repeated in the body.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// The human readable summary of the failure.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// The individual failures, one entry per rejected field or rule.
    /// </summary>
    public ProblemResponseError[] Errors { get; set; } = [];
}

/// <summary>
/// A single entry in a problem response's <c>errors</c> node.
/// </summary>
public sealed class ProblemResponseError
{
    /// <summary>
    /// The machine readable error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// The field the failure relates to, when there is one.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// The human readable description of the failure.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
