using Pot.App.Errors;
using System.Text.Json;

namespace Pot.App.Extensions;

public static class ApiDetailsErrorExtensions
{
    // The response is camelCase, and these keys reach the wire as written. An anonymous type would be renamed
    // by PropertyNamingPolicy, but a Dictionary<string, object?> is not: JsonSerializerDefaults.Web sets
    // PropertyNamingPolicy and leaves DictionaryKeyPolicy null, so dictionary keys are emitted verbatim.
    // Naming them camelCase here is what keeps every entry in the 'errors' node spelled the same way,
    // whether or not the error carries custom state.
    private const string ErrorCodeKey = "errorCode";
    private const string ErrorMessageKey = "errorMessage";
    private const string PropertyNameKey = "propertyName";

    // Only report the properties of interest (not the base class properties)
    public static object GetErrorDetails(this ApiBasicError error)
    {
        // See ValidationFailureExtensions.AddCustomState() for how custom state is added
        var details = CreateCustomStateDetails(error.CustomState);

        details[ErrorCodeKey] = error.ErrorCode;
        details[ErrorMessageKey] = error.ErrorMessage;

        return details;
    }

    public static object GetErrorDetails(this ApiDetailError error)
    {
        // See ValidationFailureExtensions.AddCustomState() for how custom state is added
        var details = CreateCustomStateDetails(error.CustomState);

        details[ErrorCodeKey] = error.ErrorCode;
        details[PropertyNameKey] = error.PropertyName;
        details[ErrorMessageKey] = error.ErrorMessage;

        return details;
    }

    private static Dictionary<string, object?> CreateCustomStateDetails(object? customState)
    {
        var details = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (customState is not IDictionary<string, object?> values)
        {
            return details;
        }

        foreach (var (key, value) in values)
        {
            // Custom state is added before the error's own properties, and both use the indexer rather than
            // Add(). Add() throws on a key already present, and this runs while an error response is being
            // written - the one place where throwing turns a handled failure into an unhandled one. Writing
            // the error's own properties last also means custom state cannot shadow them.
            details[JsonNamingPolicy.CamelCase.ConvertName(key)] = value;
        }

        return details;
    }
}
