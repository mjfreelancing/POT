using System.Text.Json;

namespace Pot.AspNetCore.Concerns.ProblemDetails;

/// <summary>
/// Builds the <c>errors</c> map a validation problem carries: one entry per rejected field, holding every message
/// for that field.
/// </summary>
internal static class ValidationErrors
{
    /// <summary>The extension that carries the map.</summary>
    internal const string Extension = "errors";

    /// <summary>
    /// The key a message takes when it belongs to the whole request rather than to one field.
    /// </summary>
    internal const string GeneralKey = "";

    /// <summary>The framework's title for a validation problem.</summary>
    internal const string Title = "One or more validation errors occurred.";

    /// <summary>
    /// One rejected field and the message that explains it.
    /// </summary>
    /// <param name="PropertyName">The field the failure belongs to, or an empty string when it belongs to the whole request.</param>
    /// <param name="ErrorMessage">The message to report for the field.</param>
    internal sealed record FieldFailure(string PropertyName, string ErrorMessage);

    /// <summary>
    /// Groups failures by field.
    /// </summary>
    /// <param name="failures">The failures to report.</param>
    /// <returns>The messages, keyed by the field they belong to.</returns>
    /// <remarks>
    /// A body field is keyed by its camelCase JSON name, which is what a caller reads in the request it sent. A name
    /// that is already the caller's key - a header, say - is passed through unchanged by its producer.
    /// </remarks>
    internal static Dictionary<string, string[]> ToMap(IEnumerable<FieldFailure> failures)
    {
        return failures
            .GroupBy(failure => ToFieldKey(failure.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
    }

    private static string ToFieldKey(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return GeneralKey;
        }

        // A nested field is reported as a dotted path, so each segment is named the way the JSON body names it.
        return string.Join('.', propertyName
            .Split('.')
            .Select(segment => JsonNamingPolicy.CamelCase.ConvertName(segment)));
    }
}
