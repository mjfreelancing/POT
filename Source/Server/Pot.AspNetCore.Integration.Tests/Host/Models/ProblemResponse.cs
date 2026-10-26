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
    /// The problem type the failure belongs to, when the endpoint declares one.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The framework's title for the failure.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The HTTP status code, repeated in the body.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// The human readable summary of the failure.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// The validation messages, keyed by the field they belong to. Only a 400 carries it.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = [];

    /// <summary>
    /// Every validation message, whatever field it belongs to.
    /// </summary>
    /// <returns>The messages, in no particular order.</returns>
    public string[] AllMessages()
    {
        return [.. Errors.Values.SelectMany(messages => messages)];
    }
}
